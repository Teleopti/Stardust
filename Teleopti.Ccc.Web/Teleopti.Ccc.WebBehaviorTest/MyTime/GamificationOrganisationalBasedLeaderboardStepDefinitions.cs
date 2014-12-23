﻿
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class GamificationOrganisationalBasedLeaderboardStepDefinitions
	{
		[Then(@"I should see only myself on the leaderboard")]
		public void ThenIShouldSeeOnlyMyselfOnTheLeaderboard()
		{
			Browser.Interactions.AssertAnyContains(".agent-badge .agent-name", DataMaker.Me().Person.Name.ToString());
			Browser.Interactions.AssertNotExistsUsingJQuery(".badge-leader-board-report", ".agent-badge .agent-rank:contains('2')");
		}

		[When(@"I open the hierarchy-picker")]
		public void WhenIOpenTheHierarchy_Picker()
		{
			Select2Box.Open("Hierarchy-Picker");
		}

		[Then(@"I should see available business hierarchy")]
		public void ThenIShouldSeeAvailableBusinessHierarchy(Table table)
		{
			var options = table.CreateSet<SingleValue>();

			foreach (var option in options)
			{
				Select2Box.AssertOptionExist("Hierarchy-Picker", option.Value);
			}
		}
		private class SingleValue
		{
			public string Value { get; set; }
		}
	}
}
