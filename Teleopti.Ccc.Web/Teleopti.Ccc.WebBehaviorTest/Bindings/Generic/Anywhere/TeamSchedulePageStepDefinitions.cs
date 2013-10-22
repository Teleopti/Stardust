using System;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
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
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .shift .layer", personName);
		}

		[Then(@"I should see '(.*)' with the schedule")]
		public void ThenIShouldSeeWithTheSchedule(string personName, Table table)
		{
			var schedule = table.CreateInstance<ScheduleInfo>();

			Browser.Interactions.AssertExistsUsingJQuery(
				string.Format(
					".person:contains('{0}') .shift .layer[data-start-time='{1}'][data-length-minutes='{2}'][style*='background-color: {3}']",
					personName,
					schedule.StartTime,
					schedule.LengthMinutes(),
					PersonSchedulePageStepDefinitions.ColorNameToCss(schedule.Color)));
		}
		
		[Then(@"I should see '(.*)' with absence")]
		public void ThenIShouldSeeWithAbsence(string personName, Table table)
		{
			var absence = table.CreateInstance<AbsenceInfo>();
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .shift .layer[style*='background-color: {1}']", personName, PersonSchedulePageStepDefinitions.ColorNameToCss(absence.Color));
		}

		[Then(@"I should see '(.*)' with no schedule")]
		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(
				".person:contains('{0}')",
				".person:contains('{0}') .shift .layer",
				personName
				);
		}

		[Then(@"I should be able to select teams")]
		public void ThenIShouldBeAbleToSelectTeams(Table table)
		{
			Browser.Interactions.AssertExists("#group-picker");

			var teams = table.CreateSet<TeamInfo>().ToArray();
			teams.ForEach(t => Browser.Interactions.AssertAnyContains("#group-picker option", t.Team));

			Browser.Interactions.AssertNotExists("#group-picker option:nth-child(" + teams.Length + ")", "#group-picker option:nth-child(" + (teams.Length + 1) + ")");
		}

		[Then(@"I should see no team available")]
		public void ThenIShouldSeeNoTeamAvailable()
		{
			Browser.Interactions.AssertExists("#group-picker");
			Browser.Interactions.AssertNotExists("#group-picker", "#group-picker option");
		}

		[Then(@"I should be able to select skills")]
		public void ThenIShouldBeAbleToSelectSkills(Table table)
		{
			Browser.Interactions.AssertExists("#skill-selector");

			var skills = table.CreateSet<SkillInfo>().ToArray();
			skills.ForEach(s => Browser.Interactions.AssertAnyContains("#skill-selector li", s.Skill));

			Browser.Interactions.AssertNotExists("#skill-selector li:nth-child(" + skills.Length + ")", "#skill-selector li:nth-child(" + (skills.Length + 1) + ")");
		}

		[Then(@"I should see a day off for '(.*)'")]
		public void ThenIShouldSeeADayOffFor(string personName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .dayoff", personName);
		}

		[Then(@"I should see '(.*)' before '(.*)'")]
		public void ThenIShouldSeeBefore(string person1, string person2)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') + .person:contains('{1}')", person1, person2);
		}

		public class TeamInfo
		{
			public string Team { get; set; }
		}

		public class AbsenceInfo
		{
			public string Color { get; set; }
		}

		public class SkillInfo
		{
			public string Skill { get; set; }
		}

		public class ScheduleInfo
		{
			public string Color { get; set; }
			public string StartTime { get; set; }
			public string EndTime { get; set; }

			public int LengthMinutes()
			{
				var result = (int)TimeSpan.Parse(EndTime).Subtract(TimeSpan.Parse(StartTime)).TotalMinutes;
				if (result < 0) result += 60 * 24;
				return result;
			}
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery("#group-picker", teamName);
		}

		public static void SelectSkill(string name)
		{
			Browser.Interactions.ClickContaining("#skill-selector li a", name);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}

		[Then(@"I should see staffing metrics for skill '(.*)'")]
		public void ThenIShouldSeeStaffingMetricsForSkill(string name)
		{
			Browser.Interactions.AssertFirstContains("#skill-selector a", name);
		}

		[Then(@"I should see staffing metrics for skill '(.*)' with")]
		public void ThenIShouldSeeStaffingMetricsForSkillWith(string name, Table table)
		{
			Browser.Interactions.AssertFirstContains("#skill-selector a", name);

			var staffingMetric = table.CreateInstance<StaffingMetricInfo>();

			Browser.Interactions.AssertAnyContains("#forecasted-hours", staffingMetric.ForecastedHours);
			Browser.Interactions.AssertAnyContains("#scheduled-hours", staffingMetric.ScheduledHours);
			Browser.Interactions.AssertAnyContains("#difference-forecasted-scheduled", staffingMetric.DifferenceHours);
			Browser.Interactions.AssertAnyContains("#difference-forecasted-scheduled", staffingMetric.DifferencePercentage);
			Browser.Interactions.AssertAnyContains("#estimated-service-level", staffingMetric.EstimatedServiceLevel);
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