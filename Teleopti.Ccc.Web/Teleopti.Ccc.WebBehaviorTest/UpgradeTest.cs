using NUnit.Framework;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[TestFixture]
	[FullIntegrationTest]
	public class UpgradeTest
	{
		[Test]
		public void ShouldRefreshClientApplicationAfterUpgrade()
		{
			Navigation.GoToPage($"wfm/#/teapot");
			Browser.Interactions.AssertFirstContains("body", "Bad coffee");

			TestControllerMethods.SetVersion("2.0");

			Browser.Interactions.AssertFirstContains("body", "I'm a teapot");
		}
	}
}