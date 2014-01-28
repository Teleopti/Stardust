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
            Browser.Interactions.AssertNotExists(".navbar .navbar-default", "a[href$='#MyReportTab']");
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

		[Then(@"I should see a message saying I dont have access to MyReport")]
		public void ThenIShouldSeeAMessageSayingIDontHaveAccess()
		{
			//todo...
			Browser.Interactions.AssertExists(".error");
		}


	}
}
