
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
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

		[Then(@"I should not see hierarchy-picker")]
		public void ThenIShouldNotSeeHierarchy_Picker()
		{
			Browser.Interactions.AssertNotExists(".BadgeLeaderBoardReport", "#Hierarchy-Picker");
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

		[Then(@"The hierarchy-picker should have '(.*)' selected")]
		public void ThenTheHierarchy_PickerShouldHaveSelected(string option)
		{
			Select2Box.AssertSelectedOptionText("Hierarchy-Picker", option);
		}

		[Then(@"I should see the ranks are")]
		public void ThenIShouldSeeTheRanksAre(Table table)
		{
			var ranks = table.CreateSet<RankInfo>();
			foreach (var rankInfo in ranks)
			{
				Browser.Interactions.AssertExistsUsingJQuery(
					string.Format(".agent-rank:contains('{0}')+.agent-name:contains('{1}')", rankInfo.Rank,
						rankInfo.Agent));
			}
		}

		[When(@"I select '(.*)' in the hierarchy-picker")]
		public void WhenISelectInTheHierarchy_Picker(string optionText)
		{
			WhenIOpenTheHierarchy_Picker();

			Select2Box.SelectItemByText("Hierarchy-Picker", optionText);
		}


		private class SingleValue
		{
			public string Value { get; set; }
		}
		private class RankInfo
		{
			public string Rank { get; set; }
			public string Agent { get; set; }
		}

	}

	
}
