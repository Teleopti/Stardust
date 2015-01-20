using System;
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
	public class IdentityAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new IdentityAuthentication(MockRepository.GenerateMock<IIdentityUserQuery>());
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