using System.Collections.Generic;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Reports
{
	[Binding]
	public sealed class ReportsStepsDefinition
	{
		[Then(@"I should see all report items")]
		public void ThenIShouldSeeAllReportItems()
		{
			Browser.Interactions.AssertExists(".test-report-list");
		}

		[Then(@"I should see leader board report table")]
		public void ThenIShouldSeeLeaderBoardReportTable()
		{
			Browser.Interactions.AssertExists(".leader-board table");
		}

		[When(@"I select date from '(.*)' to '(.*)'")]
		public void WhenISelectDateFromTo(string from, string to)
		{
			var newPeriodStr = "{" + "startDate : " + string.Format("new Date('{0}')", from) + ", " + " endDate: " +
				string.Format("new Date('{0}')", to) + "}";

			Browser.Interactions.AssertScopeValue(".leaderboard-container", "vm.isLoading", false);
			Browser.Interactions.SetScopeValues(".leaderboard-container", new Dictionary<string, string>
			{
				{ "vm.selectedPeriod", newPeriodStr}
			}, false, "vm.afterSelectedDateChange");
		}
	}
}
