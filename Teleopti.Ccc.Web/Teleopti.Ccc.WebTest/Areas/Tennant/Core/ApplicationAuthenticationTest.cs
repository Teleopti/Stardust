﻿using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserQuery>(), new PasswordVerifier(new OneWayEncryption()), new successfulPasswordPolicy());
			var res = target.Logon("nonExisting", string.Empty);
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo {Password = "thePassword"};
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new ApplicationUserQueryResult
			{
				PersonInfo = personInfo,
				PasswordPolicy = new PasswordPolicyForUser(personInfo)
			});

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption()), new successfulPasswordPolicy());
			var res = target.Logon(userName, "invalidPassword");

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		//remove this and replace with real tests when old password policy is converted!
		[Test]
		public void PasswordPolicyShouldBeChecked()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo {Id = Guid.NewGuid(), Password = encryptPasswordToDbFormat(password)};
			var queryResult = new ApplicationUserQueryResult
			{
				PersonInfo = personInfo,
				PasswordPolicy = new PasswordPolicyForUser(personInfo)
			};
			var theUserDetail = new UserDetail(null);
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(queryResult);
			var convertDataToOldUserDetailDomain = MockRepository.GenerateMock<IConvertDataToOldUserDetailDomain>();
			convertDataToOldUserDetailDomain.Expect(
				x => x.Convert(queryResult.PasswordPolicy)).Return(theUserDetail);
			var checkPasswordChange = MockRepository.GenerateMock<ICheckPasswordChange>();
			checkPasswordChange.Expect(x => x.Check(theUserDetail))
				.Return(new AuthenticationResult {HasMessage = true, Message = "THEMESSAGE", Successful = false});

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption()), new PasswordPolicyCheck(convertDataToOldUserDetailDomain, checkPasswordChange));

			var res = target.Logon(userName, password);
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo("THEMESSAGE");
		}

		[Test]
		public void LockedUserShouldFail()
		{
			const string userName = "validUserName";
			const string password = "somePassword";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			var personInfo = new PersonInfo {Password = encryptPasswordToDbFormat(password)};
			var passwordPolicy = new PasswordPolicyForUser(personInfo);
			passwordPolicy.Lock();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new ApplicationUserQueryResult
			{
				PersonInfo = personInfo,
				PasswordPolicy = passwordPolicy
			});

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption()), new successfulPasswordPolicy());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedAccountIsLocked);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var personInfo = new PersonInfo {Password = encryptPasswordToDbFormat(password), Id = Guid.NewGuid()};
			var queryResult = new ApplicationUserQueryResult
			{
				PersonInfo = personInfo,
				PasswordPolicy = new PasswordPolicyForUser(personInfo)
			};
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(queryResult);

			var target = new ApplicationAuthentication(findApplicationQuery, new PasswordVerifier(new OneWayEncryption()), new successfulPasswordPolicy());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tennant.Should().Be.EqualTo(queryResult.PersonInfo.Tennant);
			res.PersonId.Should().Be.EqualTo(queryResult.PersonInfo.Id);
		}


		private static string encryptPasswordToDbFormat(string visiblePassword)
		{
			return new OneWayEncryption().EncryptString(visiblePassword);
		}

		private class successfulPasswordPolicy : IPasswordPolicyCheck
		{
			public bool Verify(PasswordPolicyForUser passwordPolicyForUser, out string passwordPolicyFailureReason)
			{
				passwordPolicyFailureReason = null;
				return true;
			}
		}
	}
}