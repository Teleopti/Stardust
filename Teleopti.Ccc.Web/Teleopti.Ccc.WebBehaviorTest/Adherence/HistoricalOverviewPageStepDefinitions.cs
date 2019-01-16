using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class HistoricalOverviewPageStepDefinitions
	{
		[When(@"I review historical adherence for team '(.*)'")]
		public void WhenIReviewHistoricalOverviewForTeam(string name)
		{
			TestControllerMethods.Logon();
			Navigation.GoToHistoricalOverview(idForTeam(name));
			// if this becomes flaky again, lets:
			// - open the team automatically if there's only one selected
			// - or pass an open=true argument. But ^^^ seems nicer
			Browser.Interactions.Click(".card-panel-header-wrapper");
		}

		[Then(@"I should see '(.*)' having adherence percent of '(.*)' on '(.*)'")]
		public void ThenIShouldSeePersonHavingAdherencePercentForDay(string name, string percent, string day)
		{
			Browser.Interactions.AssertFirstContains($".agent-adherence-period[data-agent='{name}'] .daily-adherence-percent[data-day='{day}']", percent);
		}

		[Then(@"I should see '(.*)' having adherence percent of '(.*)' for period")]
		public void ThenIShouldSeePersonHavingAdherencePercentOfForPeriod(string name, string percent)
		{
			Browser.Interactions.AssertFirstContains($".agent-adherence-period[data-agent='{name}'] .adherence-value", percent);
		}

		[Then(@"I should see '(.*)' is late for work in total of '(.*)' minutes for '(.*)' times during period")]
		public void ThenIShouldSeePersonIsLateForWorkInTotalOfMinutesAndTimes(string name, string minutes, string times)
		{
			Browser.Interactions.AssertFirstContains($".agent-adherence-period[data-agent='{name}'] .late-for-work", times);
			Browser.Interactions.AssertFirstContains($".agent-adherence-period[data-agent='{name}'] .minutes-late-for-work", minutes);
		}

		private static Guid idForTeam(string teamName)
		{
			var teamId = (from t in DataMaker.Data().UserDatasOfType<TeamConfigurable>()
				let team = t.Team
				where team.Description.Name.Equals(teamName)
				select team.Id.GetValueOrDefault()).First();
			return teamId;
		}
	}
}