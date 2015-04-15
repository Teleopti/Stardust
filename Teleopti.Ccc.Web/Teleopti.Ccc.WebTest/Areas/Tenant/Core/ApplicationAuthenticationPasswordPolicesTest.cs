﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
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
			personInfo.SetPassword("thePassword");
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new SuccessfulPasswordPolicy(), new DataSourceConfigurationProviderFake());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");

			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(2);
		}

		[Test]
		public void TooManyInvalidAttemptsShouldLockUser()
		{
			const string userName = "validUserName";
			var personInfo = new PersonInfo();
			personInfo.SetPassword("thePassword");
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);
			pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), new DataSourceConfigurationProviderFake());
			target.Logon(userName, "invalidPassword");
			passwordPolicyForUser.IsLocked.Should().Be.False();
			target.Logon(userName, "invalidPassword");
			passwordPolicyForUser.IsLocked.Should().Be.True();
		}

		[Test]
		public void SuccessfulLogonShouldStartNewSequence()
		{
			const string userName = "validUserName";
			const string password = "adsfasdf";
			var personInfo = new PersonInfo();
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), new DataSourceConfigurationProviderFake());
			target.Logon(userName, "invalidPassword");
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(1);
			target.Logon(userName, password);
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(0);
		}


		[Test]
		public void WhenTimePassedShouldStartNewSequence()
		{
			const string userName = "validUserName";
			const string password = "adsfasdf";
			var personInfo = new PersonInfo();
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(100);
			pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), new DataSourceConfigurationProviderFake());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(2);

			//logon two hours later
			var inTwoHours = MockRepository.GenerateMock<INow>();
			inTwoHours.Expect(x => inTwoHours.UtcDateTime()).Return(DateTime.Now.AddHours(2));
			var target2 = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, inTwoHours),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<IDataSourceConfigurationProvider>());
			target2.Logon(userName, "invalidPassword");
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(1);
		}

		[Test]
		public void LockedUserShouldFail()
		{
			const string userName = "validUserName";
			const string password = "somePassword";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo();
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var passwordPolicy = new PasswordPolicyForUser(personInfo);
			passwordPolicy.Lock();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicy);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new SuccessfulPasswordPolicy(), new DataSourceConfigurationProviderFake());
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
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(passwordPolicyForUser))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = true, PasswordExpired = false });

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new PasswordPolicyCheck(checkPasswordChange), new DataSourceConfigurationProviderFake());

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
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(passwordPolicyForUser))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = false, PasswordExpired = true});

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new PasswordPolicyCheck(checkPasswordChange), new DataSourceConfigurationProviderFake());

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
			personInfo.SetPassword(EncryptPassword.ToDbFormat(password));
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var checkPasswordChange = MockRepository.GenerateMock<ITenantCheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(passwordPolicyForUser))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = false });

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new PasswordPolicyCheck( checkPasswordChange), new DataSourceConfigurationProviderFake());

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		} 
	}
}