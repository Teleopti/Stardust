﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tennant.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class ApplicationAuthenticationPasswordPolicesTest
	{
		[Test]
		public void IncorrectPasswordShouldIncreaseInvalidAttempts()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(new PersonInfo { Password = "thePassword" });
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");

			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(2);
		}

		[Test, Ignore("Roger is looking at this one")]
		public void TooManyInvalidAttemptsShouldLockUser()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(new PersonInfo { Password = "thePassword" });
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
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

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(new PersonInfo { Password = EncryptPassword.ToDbFormat(password) });
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(1);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
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

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var passwordPolicyForUser = new PasswordPolicyForUser(new PersonInfo { Password = EncryptPassword.ToDbFormat(password) });
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var pwPolicy = MockRepository.GenerateStub<IPasswordPolicy>();
			pwPolicy.Expect(x => x.MaxAttemptCount).Return(100);
			pwPolicy.Expect(x => x.InvalidAttemptWindow).Return(TimeSpan.FromHours(1));

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			target.Logon(userName, "invalidPassword");
			target.Logon(userName, "invalidPassword");
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(2);

			//logon two hours later
			var inTwoHours = MockRepository.GenerateMock<INow>();
			inTwoHours.Expect(x => inTwoHours.UtcDateTime()).Return(DateTime.Now.AddHours(2));
			var target2 = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => pwPolicy, inTwoHours),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			target2.Logon(userName, "invalidPassword");
			passwordPolicyForUser.InvalidAttempts.Should().Be.EqualTo(1);
		}

		[Test]
		public void LockedUserShouldFail()
		{
			const string userName = "validUserName";
			const string password = "somePassword";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo { Password = EncryptPassword.ToDbFormat(password) };
			var passwordPolicy = new PasswordPolicyForUser(personInfo);
			passwordPolicy.Lock();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicy);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new SuccessfulPasswordPolicy(), MockRepository.GenerateMock<INHibernateConfigurationsHandler>());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedAccountIsLocked);
		}


		//remove this and replace with real tests when old password policy is converted!
		[Test]
		public void PasswordPolicyShouldBeChecked()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo { Id = Guid.NewGuid(), Password = EncryptPassword.ToDbFormat(password) };
			var passwordPolicyForUser = new PasswordPolicyForUser(personInfo);
			var theUserDetail = new UserDetail(null);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(passwordPolicyForUser);
			var convertDataToOldUserDetailDomain = MockRepository.GenerateMock<IConvertDataToOldUserDetailDomain>();
			convertDataToOldUserDetailDomain.Expect(
				x => x.Convert(passwordPolicyForUser)).Return(theUserDetail);
			var checkPasswordChange = MockRepository.GenerateMock<ICheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(theUserDetail))
				.Return(new AuthenticationResult { HasMessage = true, Message = "THEMESSAGE", Successful = false });

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption(), () => new DummyPasswordPolicy(), new Now()),
				new PasswordPolicyCheck(convertDataToOldUserDetailDomain, checkPasswordChange),
					MockRepository.GenerateMock<INHibernateConfigurationsHandler>());

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		} 
	}
}