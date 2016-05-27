﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.TeamSchedule
{
	[Binding]
	public sealed class TeamScheduleStepDefinitions
	{
		[When(@"I searched schedule with keyword '(.*)' and schedule date '(.*)'")]
		public void WhenISearchedScheduleWithKeywordAndScheduleDate(string searchkeyword, DateTime scheduleDate)
		{
			Browser.Interactions.FillWith("#simple-people-search", string.Format("\"{0}\"", searchkeyword));
			Browser.Interactions.PressEnter("#simple-people-search");

			var propertyValues = new Dictionary<string, string>
			{
				{"vm.scheduleDate", string.Format("new Date('{0}')", scheduleDate.ToShortDateString())}
			};
			Browser.Interactions.SetScopeValues(".datepicker-container", propertyValues);
			Browser.Interactions.PressEnter("team-schedule-datepicker #teamschedule-datepicker-input");
			Browser.Interactions.InvokeServiceAction(".team-schedule", "ScenarioTestUtil", "inScenarioTest");
		}

		[Then(@"I should see schedule with absence '(.*)' for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleForDisplayed(string absence, string agentName)
		{
			Browser.Interactions.AssertAnyContains(".person-name", agentName);
			Browser.Interactions.AssertVisibleUsingJQuery($".schedule .layer.personAbsence[projection-name='{absence}']");
		}

		[Then(@"I should see schedule with no absence for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleWithNoAbsenceForDisplayed(string agentName)
		{
			Browser.Interactions.AssertNotExists(".person-name", ".schedule div.personAbsence");
		}

		[When(@"I selected the person absence for '(.*)'")]
		public void WhenISelectedThePersonAbsenceFor(string agentName)
		{
			Browser.Interactions.ClickContaining(".person-name .wfm-checkbox-label", "John Smith");
		}

		[When(@"I selected agent '(.*)'")]
		public void WhenISelectedAgent(string agentName)
		{
			Browser.Interactions.ClickContaining(".person-name", agentName);
		}
		
		[When(@"I open menu in team schedule")]
		public void WhenIOpenMenuInTeamSchedule()
		{
			Browser.Interactions.Click("#scheduleContextMenuButton");
		}

		[Then(@"I should see '(.*)' menu item is disabled")]
		public void ThenIShouldSeeMenuItemIsDisabled(string menuName)
		{
			Browser.Interactions.AssertVisibleUsingJQuery($"#menuItem{menuName}[disabled]");
		}

		[Then(@"I should see '(.*)' menu is enabled")]
		public void ThenIShouldSeeMenuIsEnabled(string menuName)
		{
			Browser.Interactions.AssertVisibleUsingJQuery($"#menuItem{menuName}:not(:disabled)");
		}

		[Then(@"I should not see '(.*)' menu item")]
		public void ThenIShouldNotSeeMenuItem(string description)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery("#scheduleContextMenuButton", $"#menuItem{description}");
		}

		[When(@"I click menu item '(.*)' in team schedule")]
		public void WhenIClickMenuItemInTeamSchedule(string menuItem)
		{
			Browser.Interactions.Click("#menuItem" + menuItem);
		}

		[When(@"I set new activity as")]
		public void WhenISetNewActivityAs(Table table)
		{
			Browser.Interactions.WaitScopeCondition(".add-activity", "vm.availableActivitiesLoaded", true,
				() =>
				{
					var values = table.CreateInstance<AddActivityFormInfo>();

					Browser.Interactions.ClickVisibleOnly(".add-activity .activity-selector");
					Browser.Interactions.ClickContaining(".activity-selector option", values.Activity);

					Browser.Interactions.ClickContaining(".activity-option-item", values.Activity);
					var startTime = string.Format("new Date('{0}')", values.StartTime);
					var endTime = string.Format("new Date('{0}')", values.EndTime);
					var timeRangeStr = string.Format("{{startTime:{0}, endTime:{1}}}", startTime, endTime);
					var selectedDate = string.Format("function(){{return new Date('{0}');}}", values.SelectedDate);
					var timeRange = new Dictionary<string, string>
					{
						{"vm.selectedDate", selectedDate},
						{"vm.timeRange", timeRangeStr}
					};
					Browser.Interactions.SetScopeValues(".add-activity .activity-time-range", timeRange);
				});
		}

		[Then(@"I should be able to apply my new activity")]
		public void ThenIShouldBeAbleToApplyMyNewActivity()
		{
			Browser.Interactions.AssertScopeValue("#applyActivity", "newActivityForm.$valid", true);
			Browser.Interactions.AssertExists("#applyActivity.wfm-btn-primary");
		}

		[Then(@"I should see the add activity time starts '(.*)' and ends '(.*)'")]
		public void ThenIShouldSeeTheAddActivityTimeStartsAndEnds(string startTime, string endTime)
		{
			var startHour = string.Format(startTime).Substring(0, 2);
			var startMin = string.Format(startTime).Substring(3, 2);
			var endHour = string.Format(endTime).Substring(0, 2);
			var endMin = string.Format(endTime).Substring(3, 2);
			Browser.Interactions.AssertIndexedInputValueUsingJQuery("activity-time-range-picker input", 0, startHour);
			Browser.Interactions.AssertIndexedInputValueUsingJQuery("activity-time-range-picker input", 1, startMin);
			Browser.Interactions.AssertIndexedInputValueUsingJQuery("activity-time-range-picker input", 2, endHour);
			Browser.Interactions.AssertIndexedInputValueUsingJQuery("activity-time-range-picker input", 3, endMin);
		}

		[When(@"I apply my new activity")]
		public void WhenIApplyMyNewActivity()
		{
			Browser.Interactions.Click("#applyActivity");
		}

		[When(@"I apply add personal activity")]
		public void WhenIApplyAddPersonalActivity()
		{
			Browser.Interactions.Click("#applyPersonalActivity");
		}

		[Then(@"I should see agent '(.*)' with shift '(.*)'")]
		public void ThenIShouldSeeAgentWithShift(string agent, string shift)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".projection-layer[projection-name={shift}]");
		}

		[Then(@"I should see a successful notice")]
		public void ThenIShouldSeeASuccessfulNotice()
		{
			Browser.Interactions.AssertExists(".notice-container .notice-success");
		}

		[Then(@"I should see a warning notice")]
		public void ThenIShouldSeeAWarningNotice()
		{
			Browser.Interactions.AssertExists(".notice-container .notice-warning");
		}

		[Then(@"I should see an error notice")]
		public void ThenIShouldSeeAnErrorNotice()
		{
			Browser.Interactions.AssertExists(".notice-container .notice-error");
		}

		[When(@"I see a successful notice")]
		public void WhenISeeASuccessfulNotice()
		{
			Browser.Interactions.AssertExists(".notice-container .notice-success");
		}


		[When(@"I try to delete selected absence")]
		public void WhenITryToDeleteSelectedAbsence()
		{
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemRemoveAbsence");
		}

		[Then(@"I should see a confirm message that will remove (\d*) absences from (\d*) person"), SetCulture("en-US")]
		public void ThenIShouldSeeDialogToConfirmAbsenceDeletion(int personCount, int personAbsenceCount)
		{
			Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
			Browser.Interactions.AssertAnyContains(".team-schedule-command-confirm-dialog",
				string.Format(Resources.AreYouSureToRemoveSelectedAbsence, personAbsenceCount, personCount));
		}

		[Then(@"I should not see remove entire cross days checkbox input")]
		public void ThenIShouldNotSeeRemoveEntireCrossDaysCheckboxInput()
		{
			Browser.Interactions.AssertNotExists(".team-schedule-command-confirm-dialog", "#checkRemoveEntireAbsence");
		}

		[When(@"I click apply button")]
		public void WhenIClickApplyButton()
		{
			Browser.Interactions.Click(".team-schedule-command-confirm-dialog .wfm-btn-primary:not([disabled])");
		}

		[When(@"I selected activity '(.*)'")]
		public void WhenISelectedActivity(string description)
		{
			Browser.Interactions.ClickUsingJQuery($".projection-layer[projection-name={description}]");
		}

		[When(@"I apply move activity")]
		public void WhenIApplyMoveActivity()
		{
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemMoveActivity");
			var newStartTime = new Dictionary<string, string>
			{
				{"vm.newStartTime", "{'startTime':'2016-10-10T10:00:00.000Z','nextDay':false}"}
			};
			Browser.Interactions.SetScopeValues(".move-activity", newStartTime);
			Browser.Interactions.ClickUsingJQuery("#applyMoveActivity");
		}

		[When(@"I apply remove activity")]
		public void WhenIApplyRemoveActivity()
		{
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemRemoveActivity");
		}

		[Then(@"I should see contract time of '(.*)'")]
		public void ThenIShouldSeeContractTimeOf(string contractTime)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".contract-time", contractTime);
		}

		[When(@"I move activity to '(.*)' with next day being '(.*)'")]
		public void WhenIMoveActivityToWithNextDayBeing(string newStartTime, string isNextDay)
		{
			var newStartTimeModel = new Dictionary<string, string>
			{
				{"vm.bindingTime", $"moment('{newStartTime}').toDate()"},
				{"vm.nextDay", isNextDay}
			};
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemMoveActivity");
			Browser.Interactions.SetScopeValues(".move-activity", newStartTimeModel);
			Browser.Interactions.ClickUsingJQuery("#applyMoveActivity");
		}

		[Then(@"the start of '(.*)' is '(.*)' mins later than the start of '(.*)'")]
		public void ThenTheStartOfIsMinsLaterThanTheStartOf(string activityA, int interval, string activityB)
		{
			var hours = Convert.ToSingle(Browser.Interactions.Javascript("return (document.querySelectorAll('.ruler').length - 1)"));
			var scheduleTableWidth = Convert.ToSingle(Browser.Interactions.Javascript("return document.querySelector('td.schedule.schedule-column').clientWidth"));
			var minuteWidth = scheduleTableWidth / hours / 60;
			var activityALeft = Convert.ToSingle(Browser.Interactions.Javascript($"return parseFloat(getComputedStyle(document.querySelector('[projection-name=\"{activityA}\"]')).left);"));
			var activityBLeft = Convert.ToSingle(Browser.Interactions.Javascript($"return parseFloat(getComputedStyle(document.querySelector('[projection-name=\"{activityB}\"]')).left);"));
			var intervalWidth = Math.Floor(minuteWidth * interval);
			Assert.AreEqual(intervalWidth, Math.Floor(activityALeft - activityBLeft));
		}
	}

	public class AddActivityFormInfo
	{
		public string Activity { get; set; }
		public string SelectedDate { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool IsNextDay { get; set; }
	}
}
