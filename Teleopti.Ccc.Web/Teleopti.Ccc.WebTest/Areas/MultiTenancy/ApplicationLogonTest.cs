using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Model;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest.Areas.MultiTenancy
{
	[TenantTest]
	public class ApplicationLogonTest
	{
		public TenantAuthenticationFake TenantAuthentication;
		public AuthenticateController Target;
		public ApplicationUserQueryFake ApplicationUserQuery;
		public LogLogonAttemptFake LogLogonAttempt;
		public TenantUnitOfWorkFake TenantUnitOfWork;
		public PasswordPolicyFake PasswordPolicy;
		public MutableNow Now;

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
			var result =
				Target.ApplicationLogon(new ApplicationLogonModel {UserName = RandomName.Make(), Password = RandomName.Make()})
					.Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void NonExistingUserShouldFailWithinSimilarTimeAsExistingUserWithWrongPassword()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);

			var existingUserWatch = new Stopwatch();
			existingUserWatch.Start();
			Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = RandomName.Make() })
					.Result<TenantAuthenticationResult>();
			existingUserWatch.Stop();

			var nonExistingUserWatch = new Stopwatch();
			nonExistingUserWatch.Start();
			Target.ApplicationLogon(new ApplicationLogonModel { UserName = RandomName.Make(), Password = RandomName.Make() })
					.Result<TenantAuthenticationResult>();
			nonExistingUserWatch.Stop();

			Math.Abs(existingUserWatch.Elapsed.Subtract(nonExistingUserWatch.Elapsed).TotalMilliseconds).Should().Be
				.LessThanOrEqualTo(10);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);

			var result =
				Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()})
					.Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var tenant = new Tenant(RandomName.Make());
			tenant.DataSourceConfiguration.SetAnalyticsConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			tenant.DataSourceConfiguration.SetApplicationConnectionString(string.Format("Initial Catalog={0}", RandomName.Make()));
			var encryptedDataSourceConfiguration = new DataSourceConfigurationEncryption().EncryptConfig(tenant.DataSourceConfiguration);

			var personInfo = new PersonInfo(tenant, Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);

			var res = Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = password}).Result<TenantAuthenticationResult>();

			res.Success.Should().Be.True();
			res.Tenant.Should().Be.EqualTo(personInfo.Tenant.Name);
			res.PersonId.Should().Be.EqualTo(personInfo.Id);
			res.DataSourceConfiguration.AnalyticsConnectionString.Should().Be.EqualTo(encryptedDataSourceConfiguration.AnalyticsConnectionString);
			res.DataSourceConfiguration.ApplicationConnectionString.Should().Be.EqualTo(encryptedDataSourceConfiguration.ApplicationConnectionString);
			res.ApplicationConfig.Single().Value
				.Should().Be.EqualTo(tenant.ApplicationConfig[tenant.ApplicationConfig.Single().Key]);
			res.TenantPassword.Should().Be.EqualTo(personInfo.TenantPassword);
		}

		[Test]
		public void ShouldLogApplicationLogonSuccessful()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);

			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = password})
				.Result<TenantAuthenticationResult>();

			LogLogonAttempt.PersonId.Should().Be.EqualTo(personInfo.Id);
			LogLogonAttempt.Successful.Should().Be.True();
			LogLogonAttempt.UserName.Should().Be.EqualTo(logonName);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void ShouldLogApplicationLogonUnsuccessful()
		{
			var logonName = RandomName.Make();

			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()})
				.Result<TenantAuthenticationResult>();

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
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);

			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});

			personInfo.ApplicationLogonInfo.InvalidAttempts
				.Should().Be.EqualTo(2);
			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void TooManyInvalidAttemptsShouldLockUser()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.MaxAttemptCount = 1;
			PasswordPolicy.InvalidAttemptWindow = TimeSpan.FromHours(1);

			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.False();
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.True();

			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void SuccessfulLogonShouldStartNewSequence()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.MaxAttemptCount = 1;

			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(1);
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = password});
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(0);

			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void WhenTimePassedShouldStartNewSequence()
		{
			var logonName = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, RandomName.Make(), new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.MaxAttemptCount = 100;
			PasswordPolicy.InvalidAttemptWindow = TimeSpan.FromHours(1);

			Now.Is(DateTime.UtcNow);
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(2);

			//logon two hours later
			Now.Is(DateTime.UtcNow.AddHours(2));
			Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = RandomName.Make()});
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(1);

			TenantUnitOfWork.WasCommitted.Should().Be.True();
		}

		[Test]
		public void LockedUserShouldFail()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			personInfo.ApplicationLogonInfo.Lock();
			ApplicationUserQuery.Has(personInfo);

			var res =
				Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = password})
					.Result<TenantAuthenticationResult>();

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedAccountIsLocked);
		}

		[Test]
		public void PasswordThatWillExpireSoonShouldSuccedButHaveFailReasonSet()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo(new Tenant(RandomName.Make()), Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.PasswordValidForDayCount = 1;
			PasswordPolicy.PasswordExpireWarningDayCount = 2;

			var result = Target.ApplicationLogon(new ApplicationLogonModel {UserName = logonName, Password = password}).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.True();
			result.FailReason.Should()
				.Be.EqualTo(string.Format(CultureInfo.CurrentUICulture, Resources.LogOnWarningPasswordWillSoonExpire, 1));
		}

		[Test]
		public void ExpiredPasswordShouldFail()
		{
			var logonName = RandomName.Make();
			var password = RandomName.Make();
			var personInfo = new PersonInfo(new Tenant(RandomName.Make()), Guid.NewGuid());
			personInfo.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, password, new OneWayEncryption());
			ApplicationUserQuery.Has(personInfo);
			PasswordPolicy.PasswordValidForDayCount = 0;

			var result = Target.ApplicationLogon(new ApplicationLogonModel { UserName = logonName, Password = password }).Result<TenantAuthenticationResult>();

			result.Success.Should().Be.False();
			result.FailReason.Should()
				.Be.EqualTo(string.Format(CultureInfo.CurrentUICulture, Resources.LogOnFailedPasswordExpired));
		}
	}
}