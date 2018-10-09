using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceHistoricalOverviewStepDefinitions
	{
		[When(@"I review historical adherence for team '(.*)'")]
		public void WhenIReviewHistoricalOverviewForTeam(string name)
		{
			TestControllerMethods.Logon();
			var teamId = idForTeam(name);
			Navigation.GoToHistoricalOverview(teamId);

			var selector = ".card-panel-header-wrapper";
			var findCardPanel = $@"
var element = document.querySelector(""{selector}"");
return 'OK';
";
			Browser.Interactions.AssertJavascriptResultContains(findCardPanel, "OK");
			Browser.Interactions.Click(selector);
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