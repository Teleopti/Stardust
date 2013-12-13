using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MyReportStepDefinition
	{
		[Then(@"MyReport tab should not be visible")]
		public void ThenReportTabShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotExists(".navbar-inner","a[href$='#MyReportTab']");
		}

		[Then(@"MyReport tab should be visible")]
		public void ThenMyReportTabShouldBeVisible()
		{
			Browser.Interactions.AssertExists("a[href$='#MyReportTab']");
		}

		[Then(@"I should see MyReport for '(.*)'")]
		public void ThenIShouldSeeMyReportFor(string date)
		{
			ScenarioContext.Current.Pending();
		}

	}
}
