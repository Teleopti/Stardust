using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Authentication.Core;

namespace Teleopti.Ccc.WebTest.Areas.Authentication.Core
{
	public class ApplicationAuthenticationTest
	{
		[Test]
		public void NonExistingUserShouldFail()
		{
			var target = new ApplicationAuthentication();
			var res = target.Logon("nonExisting", string.Empty);
			
			res.Success.Should().Be.False();
			res.FailReason.Should().Be.EqualTo(Resources.LogOnFailedInvalidUserNameOrPassword);
		}
	}
}