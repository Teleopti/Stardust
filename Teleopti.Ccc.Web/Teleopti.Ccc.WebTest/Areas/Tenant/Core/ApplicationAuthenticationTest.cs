using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserQuery>(),
				new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()), new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>(),null);
			var res = target.Logon("nonExisting", string.Empty);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo();
			personInfo.SetPassword("thePassword");
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new PasswordPolicyForUser(personInfo));

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>(),null);
			var res = target.Logon(userName, "invalidPassword");

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var dataSourceConfiguration = new DataSourceConfiguration();
			var personInfo = new PersonInfo { Id = Guid.NewGuid()};
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var pwPolicyLoader = MockRepository.GenerateMock<ILoadPasswordPolicyService>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new PasswordPolicyForUser(personInfo));
			pwPolicyLoader.Expect(x => x.DocumentAsString).Return("somepolicy");
			var dataSourceProvider = MockRepository.GenerateStub<IDataSourceConfigurationProvider>();
			dataSourceProvider.Stub(x => x.ForTenant(personInfo.Tenant)).Return(dataSourceConfiguration);
			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), dataSourceProvider, pwPolicyLoader);
			
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(dataSourceConfiguration);
			res.PasswordPolicy.Should().Be.EqualTo("somepolicy");
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo { Id = Guid.NewGuid() };
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var queryResult = new PasswordPolicyForUser(personInfo);
			
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(queryResult);
			var nhibHandler = MockRepository.GenerateMock<IDataSourceConfigurationProvider>();
			nhibHandler.Stub(x => x.ForTenant(queryResult.PersonInfo.Tenant)).Return(null);
			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => MockRepository.GenerateStub<IPasswordPolicy>(), new Now()),
				new SuccessfulPasswordPolicy(), nhibHandler, null);

			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}
	}
}