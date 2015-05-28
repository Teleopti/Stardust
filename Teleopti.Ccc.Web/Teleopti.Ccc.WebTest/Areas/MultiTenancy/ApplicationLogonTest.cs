using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.Areas.MultiTenancy.Core;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ApplicationLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;
		public ApplicationUserQueryFake ApplicationUserQuery;
		public DataSourceConfigurationProviderFake DataSourceConfigurationProvider;
		public LogLogonAttemptFake LogLogonAttempt;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public PasswordPolicyFake PasswordPolicy;

		[Test]
		public void ShouldAcceptAccessWithoutTenantCredentials()
		{
			TenantAuthentication.NoAccess();

			Assert.DoesNotThrow(() =>
				Target.ApplicationLogon(new ApplicationLogonModel {UserName = RandomName.Make(), Password = RandomName.Make()}));
		}

		[Test]
		public void NonExistingUserShouldFail()
		{
			var result = Target.ApplicationLogon(new ApplicationLogonModel {UserName = RandomName.Make(), Password = RandomName.Make()}).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			var logonName = RandomName.Make();
			var personInfo= new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());
			ApplicationUserQuery.Has(personInfo);

			var result = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldFailIfNoDatasource()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password);
			ApplicationUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(new Tenant("another tenant"), new DataSourceConfiguration());

			var result = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.NoDatasource);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var tenant = new Tenant(RandomName.Make());
			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			var dataSourceConfiguration = new DataSourceConfiguration();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password);
			ApplicationUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(tenant, dataSourceConfiguration);

			var res = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password }).Result<TenantAuthenticationResult>();

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant.Name);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.Should().Be.SameInstanceAs(dataSourceConfiguration);
			res.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}

		[Test]
		public void ShouldLogApplicationLogonSuccessful()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password);
			var datasourceConfiguration = new DataSourceConfiguration();
			ApplicationUserQuery.Has(personInfo);
			DataSourceConfigurationProvider.Has(personInfo.Tenant, datasourceConfiguration);

			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password}).Result<TenantAuthenticationResult>();

			LogLogonAttempt.PersonId.Should().Be.EqualTo(personInfo.Id);
			LogLogonAttempt.Successful.Should().Be.True();
			LogLogonAttempt.UserName.Should().Be.EqualTo(logonName);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldLogApplicationLogonUnsuccessful()
		{
			var logonName= RandomName.Make();

			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() }).Result<TenantAuthenticationResult>();

			LogLogonAttempt.PersonId.Should().Be.EqualTo(Guid.Empty);
			LogLogonAttempt.Successful.Should().Be.False();
			LogLogonAttempt.UserName.Should().Be.EqualTo(logonName);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void IncorrectPasswordShouldIncreaseInvalidAttempts()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());
			ApplicationUserQuery.Has(personInfo);

			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() });
			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() });

			personInfo.ApplicationLogonInfo.InvalidAttempts
				.Should().Be.EqualTo(2);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void TooManyInvalidAttemptsShouldLockUser()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.MaxAttemptCount = 1;
			PasswordPolicy.InvalidAttemptWindow = TimeSpan.FromHours(1);

			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() });
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.False();
			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() });
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.True();

			TenantUnitOfWork.WasCommitted.Should().Be.True();


			//const string userName = "validUserName";
			//var personInfo = new PersonInfo();
			//personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), RandomName.Make(), "thePassword");
			//var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			//findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			//var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			//pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);
			//pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			//var target = new ApplicationAuthentication(findApplicationQuery,
			//	new DataSourceConfigurationProviderFake(), () => pwPolicy, new Now(), new SuccessfulPasswordPolicy());
			//target.Logon(userName, "invalidPassword");
			//personInfo.ApplicationLogonInfo.IsLocked.Should().Be.False();
			//target.Logon(userName, "invalidPassword");
			//personInfo.ApplicationLogonInfo.IsLocked.Should().Be.True();
		}
	}
}