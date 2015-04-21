using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tenant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tenant.Core
{
	public class ApplicationAuthenticationPasswordPolicesTest
	{
		[Test]
		public void IncorrectPasswordShouldIncreaseInvalidAttempts()
		{
			const string userName = "validUserName";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), "thePassword");
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => new DummyPasswordPolicy(), new Now(), new SuccessfulTenantCheckPasswordChange());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");

			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(2);
		}

		[Test]
		public void TooManyInvalidAttemptsShouldLockUser()
		{
			const string userName = "validUserName";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), "thePassword");
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);
			pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => pwPolicy, new Now(), new SuccessfulTenantCheckPasswordChange());
			target.Logon(userName, "invalidPassword");
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.False();
			target.Logon(userName, "invalidPassword");
			personInfo.ApplicationLogonInfo.IsLocked.Should().Be.True();
		}

		[Test]
		public void SuccessfulLogonShouldStartNewSequence()
		{
			const string userName = "validUserName";
			const string password = "adsfasdf";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => pwPolicy, new Now(), new SuccessfulTenantCheckPasswordChange());
			target.Logon(userName, "invalidPassword");
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(1);
			target.Logon(userName, password);
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(0);
		}


		[Test]
		public void WhenTimePassedShouldStartNewSequence()
		{
			const string userName = "validUserName";
			const string password = "adsfasdf";
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(100);
			pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => pwPolicy, new Now(), new SuccessfulTenantCheckPasswordChange());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(2);

			//logon two hours later
			var inTwoHours = MockRepository.GenerateMock<INow>();
			inTwoHours.Expect(x => inTwoHours.UtcDateTime()).Return(DateTime.Now.AddHours(2));
			var target2 = new ApplicationAuthentication(findApplicationQuery,
				MockRepository.GenerateMock<IDataSourceConfigurationProvider>(), () => pwPolicy, inTwoHours, new SuccessfulTenantCheckPasswordChange());
			target2.Logon(userName, "invalidPassword");
			personInfo.ApplicationLogonInfo.InvalidAttempts.Should().Be.EqualTo(1);
		}

		[Test]
		public void LockedUserShouldFail()
		{
			const string userName = "validUserName";
			const string password = "somePassword";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			var personInfo = new PersonInfo();
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			personInfo.ApplicationLogonInfo.Lock();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => new DummyPasswordPolicy(), new Now(), new SuccessfulTenantCheckPasswordChange());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedAccountIsLocked);
		}

		[Test]
		public void PasswordThatWillExpireSoonShouldSuccedButHaveFailReasonSet()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			const string tenant = "theTenant";
			var personInfo = new PersonInfo(new Infrastructure.MultiTenancy.Server.Tenant(tenant)) { Id = Guid.NewGuid()};
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = true, PasswordExpired = false });

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => new DummyPasswordPolicy(), new Now(), checkPasswordChange);

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
			var personInfo = new PersonInfo { Id = Guid.NewGuid()};
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = false, PasswordExpired = true});

			var target = new ApplicationAuthentication(findApplicationQuery, new DataSourceConfigurationProviderFake(), () => new DummyPasswordPolicy(), new Now(), checkPasswordChange);

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
			var personInfo = new PersonInfo { Id = Guid.NewGuid() };
			personInfo.SetApplicationLogonCredentials(RandomName.Make(), password);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserTenantQuery>();
			findApplicationQuery.Expect(x => x.Find(userName)).Return(personInfo);
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(personInfo.ApplicationLogonInfo))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = false });

			var target = new ApplicationAuthentication(findApplicationQuery,
				new DataSourceConfigurationProviderFake(), () => new DummyPasswordPolicy(), new Now(), checkPasswordChange);

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		} 
	}
}