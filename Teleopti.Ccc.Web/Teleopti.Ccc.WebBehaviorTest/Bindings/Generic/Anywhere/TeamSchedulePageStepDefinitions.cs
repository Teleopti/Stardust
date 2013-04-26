using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.jQuery;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;
using TableRow = TechTalk.SpecFlow.TableRow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class TeamSchedulePageStepDefinitions
	{
		[Then(@"I should be viewing schedules for '(.*)'")]
		public void ThenIShouldSeePersonScheduleForPersonOnDate(string date)
		{
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see person '(.*)'")]
		public void ThenIShouldSeePerson(string personName)
		{
			Browser.Interactions.AssertExists(string.Format(".person:contains('{0}')", personName));
		}

		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			Browser.Interactions.AssertExists(string.Format(".person:contains('{0}') .shift li", personName));
		}

		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			Browser.Interactions.AssertNotExists(
				string.Format(".person:contains('{0}')", personName), 
				string.Format(".person:contains('{0}') .shift li", personName)
				);
		}

		[Then(@"I should be able to select teams")]
		public void ThenIShouldBeAbleToSelectTeams(Table table)
		{
			var select = Browser.Current.SelectList(Find.BySelector("#team-selector"));
			EventualAssert.That(() => select.Exists, Is.True);

			var teams = table.CreateSet<TeamInfo>();
			teams.ForEach(t => EventualAssert.That(() => select.Option(Find.BySelector(string.Format(":contains('{0}')", t.Team))).Exists, Is.True));
		}
		
		public class TeamInfo
		{
			public string Team { get; set; }
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Current.SelectList(Find.BySelector("#team-selector")).SelectNoWait(teamName);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}
	}
}