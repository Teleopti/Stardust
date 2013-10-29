using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class SignOutStepDefinitions
	{
		[When(@"I sign out")]
		public void WhenISignOut()
		{
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#signout");
			Browser.Interactions.AssertUrlContains("Authentication");
		}

		[When(@"I press back in the web browser")]
		public void WhenIPressBackInTheWebBrowser()
		{
			Browser.Interactions.Javascript("history.back();");
		}

	}
}