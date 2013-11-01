using System;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class TeamSchedulePageStepDefinitions
	{
		[When(@"I search for group '(.*)'")]
		public void WhenISearchForGroup(string searchText)
		{
			Select2Box.Open("group-picker");
			Browser.Interactions.Javascript("$('.select2-input').focus().val('" + searchText + "').trigger('keyup-change');");
		}

		[When(@"I click description toggle button")]
		public void WhenIClickDescriptionToggleButton()
		{
			DescriptionToggle.EnsureIsOn();
		}

		[When(@"I select the schedule activity for '(.*)' with start time '(.*)'")]
		public void WhenISelectTheScheduleActivityForWithStartTime(string name, string startTime)
		{
			Browser.Interactions.ClickUsingJQuery(string.Format(".person:contains('{0}') .shift .layer[data-start-time~='{1}']", name, startTime));
		}

		[Then(@"I should see schedule activity details for '(.*)' with")]
		public void ThenIShouldSeeScheduleActivityDetailsForWith(string name, Table table)
		{
			var scheduleActivity = table.CreateInstance<ScheduleActivityInfo>();
			var selector = string.Format(".person:contains('{0}') .activity-details", name);
			Browser.Interactions.AssertAnyContains(selector, scheduleActivity.Name);
			Browser.Interactions.AssertAnyContains(selector, scheduleActivity.StartTime);
			Browser.Interactions.AssertAnyContains(selector, scheduleActivity.EndTime);
		}

		[Then(@"I should see options include '(.*)'")]
		public void ThenIShouldSeeOptionsInclude(string text)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".select2-result .select2-result-label:contains('{0}')", text));
		}

		[Then(@"I should see options not include '(.*)'")]
		public void ThenIShouldSeeOptionsNotInclude(string text)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".select2-results", string.Format(".select2-result .select2-result-label:contains('{0}')", text));
		}

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

		[Then(@"I should see myself")]
		public void ThenIShouldSeeMyself()
		{
			Browser.Interactions.AssertAnyContains(".person", DataMaker.Data().MePerson.Name.ToString());
		}

		[Then(@"I should not see person '(.*)'")]
		public void ThenIShouldSeeNotPerson(string personName)
		{
			Browser.Interactions.AssertNotExists(".person", personName);
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
			var layer = table.CreateInstance<LayerInfo>();

			Browser.Interactions.AssertExistsUsingJQuery(
					".person:contains('{0}') .shift .layer[data-start-time='{1}'][data-length-minutes='{2}'][style*='background-color: {3}']",
					personName,
					layer.StartTime,
					layer.LengthMinutes(),
					PersonSchedulePageStepDefinitions.ColorNameToCss(layer.Color)
					);
		}
		
		[Then(@"I should see '(.*)' with absence")]
		public void ThenIShouldSeeWithAbsence(string personName, Table table)
		{
			var layer = table.CreateInstance<LayerInfo>();
			string selector;

			if (layer.StartTime != null)
			{
				selector = string.Format(".person:contains('{0}') .shift .layer[data-start-time='{1}'][data-length-minutes='{2}'][style*='background-color: {3}']",
				                         personName,
				                         layer.StartTime,
				                         layer.LengthMinutes(),
				                         PersonSchedulePageStepDefinitions.ColorNameToCss(layer.Color));
			}
			else
			{
				selector = string.Format(".person:contains('{0}') .shift .layer[style*='background-color: {1}']",
				                         personName,
				                         PersonSchedulePageStepDefinitions.ColorNameToCss(layer.Color));
			}
			if (layer.Description != null)
			{
				DescriptionToggle.EnsureIsOn();
				Browser.Interactions.AssertFirstContainsUsingJQuery(selector, layer.Description);
			}
			else
				Browser.Interactions.AssertExistsUsingJQuery(selector);
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

		[Then(@"I should be able to select groups")]
		public void ThenIShouldBeAbleToSelectGroups(Table table)
		{
			Select2Box.Open("group-picker");
			var options = table.CreateSet<GroupInfo>();
			foreach (var option in options)
			{
				Select2Box.AssertOptionExist("group-picker", option.Group);
			}
		}

		[Then(@"the group picker should have '(.*)' selected")]
		public void ThenTheGroupPickerShouldHaveSelected(string option)
		{
			Select2Box.AssertSelectedOptionText("group-picker", option);
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
			Browser.Interactions.Click("#skill-selector");

			var skills = table.CreateSet<SkillInfo>().ToArray();
			skills.ForEach(s => Browser.Interactions.AssertAnyContains("#skill-selector li", s.Skill));

			Browser.Interactions.AssertNotExists("#skill-selector li:nth-child(" + skills.Length + ")", "#skill-selector li:nth-child(" + (skills.Length + 1) + ")");
		}


		[Then(@"I should see '(.*)' with a day off")]
		public void ThenIShouldSeeADayOffFor(string personName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .dayoff", personName);
		}

		[Then(@"I should see '(.*)' with a day off named '(.*)'")]
		public void ThenIShouldSeeADayOffFor(string personName, string dayOff)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".person:contains('{0}') .dayoff:contains('{1}')", personName, dayOff);
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

		public class GroupInfo
		{
			public string Group { get; set; }
		}

		public class SkillInfo
		{
			public string Skill { get; set; }
		}

		public class LayerInfo
		{
			public string Color { get; set; }
			public string StartTime { get; set; }
			public string EndTime { get; set; }
			public string Description { get; set; }

			public int LengthMinutes()
			{
				var result = (int)TimeSpan.Parse(EndTime).Subtract(TimeSpan.Parse(StartTime)).TotalMinutes;
				if (result < 0) result += 60 * 24;
				return result;
			}
		}

		[When(@"I select group '(.*)'")]
		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Interactions.SelectOptionByTextUsingJQuery("#group-picker", teamName);
		}

		public static void SelectSkill(string name)
		{
			Browser.Interactions.Click("#skill-selector");
			Browser.Interactions.ClickContaining("#skill-selector li a", name);
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(DateTime date)
		{
			Browser.Interactions.Javascript(string.Format("test.callViewMethodWhenReady('teamschedule', 'setDateFromTest', '{0}');", date));
		}

		[Then(@"I should see group '(.*)' before '(.*)'")]
		public void ThenIShouldSeeGroupBefore(string group1, string group2)
		{
			Browser.Interactions.AssertExistsUsingJQuery("optgroup[label*='{0}'] + optgroup[label*='{1}']", group1, group2);
		}

		[Then(@"I should see option '(.*)' before '(.*)'")]
		public void ThenIShouldSeeOptionBefore(string option1, string option2)
		{
			Browser.Interactions.AssertExistsUsingJQuery("option:contains('{0}') + option:contains('{1}')", option1, option2);
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

	public class ScheduleActivityInfo
	{
		public string Name { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
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