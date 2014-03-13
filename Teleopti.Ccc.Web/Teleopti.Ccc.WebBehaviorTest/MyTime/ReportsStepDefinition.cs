using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class ReportsSteps
    {
		 [When(@"I click reports menu")]
		 public void WhenIClickReportsMenu()
		 {
			 Browser.Interactions.Click("#reports");
		 }

		 [Then(@"I should only see report '(.*)'")]
		 public void ThenIShouldOnlySeeReport(string name)
		 {
			 Browser.Interactions.AssertFirstContains(".report-list", name);
			 Browser.Interactions.AssertJavascriptResultContains(string.Format("return $('.report-list ').length === 1;"), "True");
		 }

    }
}
