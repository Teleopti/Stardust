using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
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
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see person '(.*)'")]
		public void ThenIShouldSeePerson(string personName)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".person:contains('{0}')", personName))).Exists, Is.True);
		}

		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".person:contains('{0}') .shift li", personName))).Exists, Is.True);
		}

		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".person:contains('{0}')", personName))).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".person:contains('{0}') .shift li", personName))).Exists, Is.False);
		}

		[Then(@"I should be able to select teams")]
		public void ThenIShouldBeAbleToSelectTeams(Table table)
		{
			var select = Browser.Current.SelectList(Find.BySelector("#team-selector"));
			EventualAssert.That(() => select.Exists, Is.True);

			var teams = table.CreateSet<TeamInfo>();
			teams.ForEach(t => EventualAssert.That(() => select.Option(Find.BySelector(string.Format(":contains('{0}')", t.Team))).Exists, Is.True));
		}

		[Then(@"I should be able to select skills")]
		public void ThenIShouldBeAbleToSelectSkills(Table table)
		{
			var skillDropDown = Browser.Current.Span(Find.BySelector("#skill-selector"));
			EventualAssert.That(() => skillDropDown.Exists, Is.True);

			var skillList = skillDropDown.List(Find.First());
			EventualAssert.That(() => skillList.Exists, Is.True);

			var skills = table.CreateSet<SkillInfo>();
			skills.ForEach(s => EventualAssert.That(() => skillList.ListItem(Find.BySelector(string.Format(":contains('{0}')", s.Skill))).Exists, Is.True));
		}
		
		public class TeamInfo
		{
			public string Team { get; set; }
		}

		public class SkillInfo
		{
			public string Skill { get; set; }
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Current.SelectList(Find.BySelector("#team-selector")).SelectNoWait(teamName);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Retrying.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}
	}
}