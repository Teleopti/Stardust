using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class PortalPageStepDefinitions
	{
		[When(@"I sign out")]
		public void WhenISignOut()
		{
			Browser.Interactions.Click(".navbar-toggle");
			Browser.Interactions.Click(".user-name-link");
			Browser.Interactions.Click("#signout");
			Browser.Interactions.AssertUrlContains("Authentication");
		}

		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			Browser.Interactions.AssertExists(".body-weekview-inner");
		}

		[Then(@"I should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			Browser.Interactions.AssertFirstContains("#licensed-to-label", UserTexts.Resources.LicensedToColon);
			Browser.Interactions.AssertFirstContains("#licensed-to-text", "Teleopti_RD");
		}

		[Then(@"I should see an application page")]
		public void ThenIShouldSeeAnApplicationPage()
		{
			Browser.Interactions.AssertExists("#signout, #signout-button");
		}

	}
}