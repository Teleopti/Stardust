using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class PortalPageStepDefinitions
	{
		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			Browser.Interactions.AssertExists(".body-weekview-inner");
		}

		[Then(@"I should see licensed to information")]
		public void ThenIShouldSeeLicensedToInformation()
		{
			Browser.Interactions.AssertFirstContains("#licensed-to-label", UserTexts.Resources.LicensedToColon);
			Browser.Interactions.AssertExists("#licensed-to-text"); 
		}

		[Then(@"I should see an application page")]
		public void ThenIShouldSeeAnApplicationPage()
		{
			Browser.Interactions.AssertExists("#regional-settings, #signout-button");
		}
	}
}