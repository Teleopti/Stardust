using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
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
			Browser.Interactions.SelectOptionByTextUsingJQuery("#team-selector", teamName);
		}

		[When(@"I select skill '(.*)'")]
		public void WhenISelectSkill(string name)
		{
			var skillDropDown = Browser.Current.Span(Find.BySelector("#skill-selector"));
			EventualAssert.That(() => skillDropDown.Exists, Is.True);

			var skillList = skillDropDown.List(Find.First());
			EventualAssert.That(() => skillList.ListItems.Count > 0, Is.True);

			skillList.ListItem(Find.BySelector(string.Format(":contains('{0}')", name))).Link(Find.First()).EventualClick();

			var selectedSkill = skillDropDown.Link(Find.First()).Span(Find.First());
			EventualAssert.That(() => selectedSkill.InnerHtml.Contains(name), Is.True);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}

		[Then(@"I should see metrics for skill '(.*)'")]
		public void ThenIShouldSeeMetricsForSkill(string name)
		{
			var skillDropDown = Browser.Current.Span(Find.BySelector("#skill-selector"));
			EventualAssert.That(() => skillDropDown.Exists, Is.True);
			var selectedSkill = skillDropDown.Link(Find.First()).Span(Find.First());
			EventualAssert.That(() => selectedSkill.InnerHtml.Contains(name), Is.True);
		}

		[Then(@"I should see metrics for skill '(.*)' with")]
		public void ThenIShouldSeeMetricsForSkillWith(string name, Table table)
		{
			var skillDropDown = Browser.Current.Span(Find.BySelector("#skill-selector"));
			EventualAssert.That(() => skillDropDown.Exists, Is.True);
			var selectedSkill = skillDropDown.Link(Find.First()).Span(Find.First());
			EventualAssert.That(() => selectedSkill.InnerHtml.Contains(name), Is.True);

			var staffingMetric = table.CreateInstance<StaffingMetricInfo>();

			EventualAssert.That(() => Browser.Current.Span(Find.BySelector("#forecasted-hours")).InnerHtml, Is.StringContaining(staffingMetric.ForecastedHours));
			EventualAssert.That(() => Browser.Current.Span(Find.BySelector("#scheduled-hours")).InnerHtml, Is.StringContaining(staffingMetric.ScheduledHours));
			EventualAssert.That(() => Browser.Current.Span(Find.BySelector("#difference-forecasted-scheduled")).InnerHtml, Is.StringContaining(staffingMetric.DifferenceHours));
			EventualAssert.That(() => Browser.Current.Span(Find.BySelector("#difference-forecasted-scheduled")).InnerHtml, Is.StringContaining(staffingMetric.DifferencePercentage));
			EventualAssert.That(() => Browser.Current.Span(Find.BySelector("#estimated-service-level")).InnerHtml, Is.StringContaining(staffingMetric.EstimatedServiceLevel));
		}


	}

	public class StaffingMetricInfo
	{
		public string ForecastedHours { get; set; }
		public string ScheduledHours { get; set; }
		public string DifferenceHours { get; set; }
		public string DifferencePercentage { get; set; }
		public string EstimatedServiceLevel { get; set; }
	}
}