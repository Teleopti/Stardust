using NUnit.Framework;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.WebTest.Areas.Global
{
	public class UserControllerTest
	{
		[Test]
		public void ShouldGetTheCurrentIdentityName()
		{
			var target = new UserController(new FakeCurrentIdentity("Pelle"));
			dynamic result = target.CurrentUser();
			Assert.AreEqual("Pelle", result.UserName);
		}
	}
}