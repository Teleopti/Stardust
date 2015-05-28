using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core
{
	public class ApplicationAuthenticationPasswordPolicesTest
	{
		[Test]
		public void PasswordThatWillExpireSoonShouldSuccedButHaveFailReasonSet()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			const string tenant = "theTenant";
			var personInfo = new PersonInfo(new Tenant(tenant), Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var checkPasswordChange = MockRepository.GenerateMock<IVerifyPasswordPolicy>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new PasswordPolicyResult { HasMessage = true, Message = "THEMESSAGE", Successful = true, PasswordExpired = false });
			var datasourceProvider = new DataSourceConfigurationProviderFake();
			datasourceProvider.Has(personInfo.Tenant, new DataSourceConfiguration());

			var target = new ApplicationAuthentication(findApplicationQuery,
				datasourceProvider, () => new PasswordPolicyFake(), new Now(), checkPasswordChange);

			var res = target.Logon(userName, password);
			res.Success.Should().Be.True();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
			res.Tenant.Should().Be.EqualTo(tenant);
		}

		[Test]
		public void ExpiredUserShouldSetPropertyOnResult()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var checkPasswordChange = MockRepository.GenerateMock<IVerifyPasswordPolicy>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new PasswordPolicyResult { HasMessage = true, Message = "THEMESSAGE", Successful = false, PasswordExpired = true});
			var datasourceProvider = new DataSourceConfigurationProviderFake();
			datasourceProvider.Has(personInfo.Tenant, new DataSourceConfiguration());

			var target = new ApplicationAuthentication(findApplicationQuery, datasourceProvider, () => new PasswordPolicyFake(), new Now(), checkPasswordChange);

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.PasswordExpired.Should().Be.True();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		}

		[Test]
		public void PasswordPolicyShouldBeChecked()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var checkPasswordChange = MockRepository.GenerateMock<IVerifyPasswordPolicy>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new PasswordPolicyResult { HasMessage = true, Message = "THEMESSAGE", Successful = false });
			var datasourceProvider = new DataSourceConfigurationProviderFake();
			datasourceProvider.Has(personInfo.Tenant, new DataSourceConfiguration());


			var target = new ApplicationAuthentication(findApplicationQuery,
				datasourceProvider, () => new PasswordPolicyFake(), new Now(), checkPasswordChange);

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		} 
	}
}