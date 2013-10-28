using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class RoutingStepDefinition
	{
		[Then(@"I should see MyTime")]
		public void ThenIShouldSeeMyTime()
		{
			Browser.Interactions.AssertExists(".body-weekview-inner");
		}

		[Then(@"I should see Mobile Reports")]
		public void ThenIShouldSeeMobileReports()
		{
			// Settings is Now preferred "home"
			Browser.Interactions.AssertExists("#report-settings-view");
		}

		[Then(@"I should see Anywhere")]
		public void ThenIShouldSeeAnywhere()
		{
			Browser.Interactions.AssertExists("#content-placeholder");
		}
	}
}
