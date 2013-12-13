using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MyReportStepDefinition
	{
		[Then(@"MyReport tab should not be visible")]
		public void ThenReportTabShouldNotBeVisible()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"MyReport tab should be visible")]
		public void ThenMyReportTabShouldBeVisible()
		{
			ScenarioContext.Current.Pending();
		}

		[Then(@"I should see MyReport for '(.*)'")]
		public void ThenIShouldSeeMyReportFor(string date)
		{
			ScenarioContext.Current.Pending();
		}

	}
}
