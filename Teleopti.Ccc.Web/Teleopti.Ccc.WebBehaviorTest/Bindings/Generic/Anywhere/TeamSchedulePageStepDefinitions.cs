using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class TeamSchedulePageStepDefinitions
	{
		[Then(@"I should be viewing schedules for '(.*)'")]
		public void ThenIShouldSeePersonScheduleForPersonOnDate(string date)
		{
			Browser.Interactions.AssertUrlContains(date.Replace("-", ""));
		}

		[Then(@"I should see person '(.*)'")]
		public void ThenIShouldSeePerson(string personName)
		{
			Browser.Interactions.AssertAnyContains(".person", personName);
		}

		[Then(@"I should see '(.*)' with schedule")]
		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .shift li", personName);
		}

		[Then(@"I should see '(.*)' with no schedule")]
		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(
				".person:contains('{0}')",
				".person:contains('{0}') .shift li",
				personName
				);
		}

		[Then(@"I should be able to select teams")]
		public void ThenIShouldBeAbleToSelectTeams(Table table)
		{
			Browser.Interactions.AssertExists("#team-selector");

			var teams = table.CreateSet<TeamInfo>().ToArray();
			teams.ForEach(t => Browser.Interactions.AssertAnyContains("#team-selector option", t.Team));

			Browser.Interactions.AssertNotExists("#team-selector option:nth-child(" + teams.Length + ")", "#team-selector option:nth-child(" + (teams.Length + 1) + ")");
		}

		[Then(@"I should see no team available")]
		public void ThenIShouldSeeNoTeamAvailable()
		{
			Browser.Interactions.AssertExists("#team-selector");
			Browser.Interactions.AssertNotExists("#team-selector", "#team-selector option");
		}

		
		public class TeamInfo
		{
			public string Team { get; set; }
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery("#team-selector", teamName);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}
	}
}