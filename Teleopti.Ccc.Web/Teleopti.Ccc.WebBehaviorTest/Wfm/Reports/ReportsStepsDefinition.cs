using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Reports
{
    [Binding]
    public sealed class ReportsStepsDefinition
    {
        [Then(@"I should see all report items")]
        public void ThenIShouldSeeAllReportItems()
        {
            Browser.Interactions.AssertExists(".report-list");
        }
    }
}
