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

		[Then(@"I should see leader board report table")]
		public void ThenIShouldSeeLeaderBoardReportTable()
		{
			Browser.Interactions.AssertExists(".leader-board table");
		}
	}
}
