using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Tennant.Core;

namespace Teleopti.Ccc.WebTest.Areas.Tennant.Core
{
	public class IdentityAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var queryResult = new ApplicationUserQueryResult
			{
				Success = false,
				PersonId = Guid.Empty,
				Tennant = ""
			};

			var identityUserQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			identityUserQuery.Stub(x => x.FindUserData("nonExisting")).Return(queryResult);
			var target = new IdentityAuthentication(identityUserQuery);
			var res = target.Logon("nonExisting");
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(string.Format(Resources.LogOnFailedIdentityNotFound, "nonExisting"));
		}

		
		[Test]
		public void ShouldSucceedIfValidCredentials()
		{
			const string identity = "validUser";
			
			var queryResult = new ApplicationUserQueryResult
			{
				Success = true,
				PersonId = Guid.NewGuid(),
				Tennant = "Teleopti"
			};
			var findIdentityQuery = MockRepository.GenerateMock<IIdentityUserQuery>();
			findIdentityQuery.Expect(x => x.FindUserData(identity)).Return(queryResult);

			var target = new IdentityAuthentication(findIdentityQuery);
			var res = target.Logon(identity);

			res.Success.Should().Be.True();
			res.Tennant.Should().Be.EqualTo(queryResult.Tennant);
			res.PersonId.Should().Be.EqualTo(queryResult.PersonId);
		}
	}
}