using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Security;
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
			var target = new ApplicationAuthentication(MockRepository.GenerateMock<IApplicationUserQuery>(), new OneWayEncryption());
			var res = target.Logon("nonExisting", string.Empty);
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void IncorrectPasswordShouldFail()
		{
			const string userName = "validUserName";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new ApplicationUserQueryResult
			{
				Success = true,
				Password = "thePassword"
			});

			var target = new ApplicationAuthentication(findApplicationQuery, new OneWayEncryption());
			var res = target.Logon(userName, "invalidPassword");

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}

		[Test]
		public void LockedUserShouldFail()
		{
			const string userName = "validUserName";
			const string password = "somePassword";

			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(new ApplicationUserQueryResult
			{
				Success = true,
				Password = encryptPasswordToDbFormat(password),
				IsLocked = true
			});

			var target = new ApplicationAuthentication(findApplicationQuery, new OneWayEncryption());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedAccountIsLocked);
		}

		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string userName = "validUserName";
			const string password = "somePassword";
			var queryResult = new ApplicationUserQueryResult
			{
				Success = true,
				Password = encryptPasswordToDbFormat(password),
				PersonId = Guid.NewGuid(),
				Tennant = Guid.NewGuid().ToString()
			};
			var findApplicationQuery = MockRepository.GenerateMock<IApplicationUserQuery>();
			findApplicationQuery.Expect(x => x.FindUserData(userName)).Return(queryResult);

			var target = new ApplicationAuthentication(findApplicationQuery, new OneWayEncryption());
			var res = target.Logon(userName, password);

			res.Success.Should().Be.True();
			res.Tennant.Should().Be.EqualTo(queryResult.Tennant);
			res.PersonId.Should().Be.EqualTo(queryResult.PersonId);
		}


		private static string encryptPasswordToDbFormat(string visiblePassword)
		{
			return new OneWayEncryption().EncryptString(visiblePassword);
		}
	}
}