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
		public void WhenISearchedScheduleWithKeywordAndScheduleDate(string searchkeyword, string scheduleDate)
		{
			Browser.Interactions.FillWith("#simple-people-search", string.Format("\"{0}\"", searchkeyword));
			Browser.Interactions.PressEnter("#simple-people-search");

			var propertyValues = new Dictionary<string, string>
			{
				{"vm.scheduleDate", string.Format("new Date('{0}')", scheduleDate)}
			};

			Browser.Interactions.SetScopeValues(".team-schedule",propertyValues);			
			Browser.Interactions.AssertScopeValue(".team-schedule","vm.scheduleDateMoment().format('YYYY-MM-DD')",scheduleDate);
			Browser.Interactions.InvokeScopeAction(".team-schedule","vm.onScheduleDateChanged");		
			Browser.Interactions.InvokeServiceAction(".team-schedule", "ScenarioTestUtil", "inScenarioTest");
		}

		[When(@"I should see schedule with absence '(.*)' for '(.*)' displayed")]
		[Then(@"I should see schedule with absence '(.*)' for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleForDisplayed(string absence, string agentName)
		{
			Browser.Interactions.AssertScopeValue(".team-schedule","vm.scheduleFullyLoaded",true);
			Browser.Interactions.AssertAnyContains(".person-name", agentName);
			Browser.Interactions.AssertExists($".schedule .layer.personAbsence[projection-name='{absence}']");
		}

		[Then(@"I should see schedule with no absence for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleWithNoAbsenceForDisplayed(string agentName)
		{
			Browser.Interactions.AssertScopeValue(".team-schedule","vm.isLoading", false);
			Browser.Interactions.AssertNotExists(".person-name", ".schedule div.personAbsence");
		}

		[When(@"I selected the person absence for '(.*)'")]
		public void WhenISelectedThePersonAbsenceFor(string agentName)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.ClickContaining(".person-name .wfm-checkbox-label", "John Smith");
				});
		}

		[When(@"I selected agent '(.*)'")]
		public void WhenISelectedAgent(string agentName)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertScopeValue(".team-schedule", "vm.scheduleFullyLoaded", true);
					Browser.Interactions.AssertScopeValue(".team-schedule", "vm.isLoading", false);
					Browser.Interactions.ClickContaining(".person-name", agentName);
				});
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
                    Browser.Interactions.ClickContaining(".md-select-menu-container md-option .md-text", values.Activity);

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
			Browser.Interactions.AssertScopeValue("#applyActivity","vm.isInputValid()",true);
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
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExistsUsingJQuery($".projection-layer[projection-name={shift}]");
				});
		}

		[Then(@"I should not see activity '(.*)' in schedule")]
		public void ThenIShouldNotSeeActivityInSchedule(string activity)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertNotExists(".schedule", $".shift .layer[projection-name={activity}]");
				});
		}

		[When(@"I should see a successful notice")]
		[Then(@"I should see a successful notice")]
		public void ThenIShouldSeeASuccessfulNotice()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExists(".notice-container .notice-success");
				});
		}

		[Then(@"I should see a warning notice")]
		public void ThenIShouldSeeAWarningNotice()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExists(".notice-container .notice-warning");
				});
		}

		[Then(@"I should see an error notice")]
		public void ThenIShouldSeeAnErrorNotice()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExists(".notice-container .notice-error");
				});
		}

		[When(@"I see a successful notice")]
		public void WhenISeeASuccessfulNotice()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExists(".notice-container .notice-success");
				});
		}

		[When(@"I close the success notice")]
		public void WhenICloseTheNotice()
		{
			Browser.Interactions.Click(".notice-container .notice-success i.pull-right");
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
			Browser.Interactions.AssertScopeValue(".team-schedule","vm.scheduleFullyLoaded",true);				
			Browser.Interactions.Click($".projection-layer[projection-name={description}]");				
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
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertExistsUsingJQuery(".contract-time", contractTime);
				});
		}

		[When(@"I move activity to '(.*)' with next day being '(.*)'")]
		public void WhenIMoveActivityToWithNextDayBeing(string newStartTime, string isNextDay)
		{
			var newStartTimeModel = new Dictionary<string, string>
			{
				{"vm.moveToTime", $"moment('{newStartTime}').toDate()"},
				{"vm.nextDay", isNextDay}
			};
			Browser.Interactions.ClickUsingJQuery("#scheduleContextMenuButton");
			Browser.Interactions.ClickUsingJQuery("#menuItemMoveActivity");
			Browser.Interactions.SetScopeValues(".move-activity", newStartTimeModel);
			Browser.Interactions.ClickUsingJQuery("#applyMoveActivity");
		}

		[When(@"I switch on show warnings toggle")]
		public void WhenISwitchOnShowWarningsToggle()
		{
			Browser.Interactions.Click("#show-warnings .wfm-switch-toggle");
		}

		[Then(@"I should see business rule warning")]
		public void ThenIShouldSeeBusinessRuleWarning()
		{
			Browser.Interactions.AssertExists(".warning-icon .mdi-account-alert");
		}

		[When(@"I click on a shift category label")]
		public void WhenIClickOnAShiftCategoryLabel()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true, () =>
			{
				Browser.Interactions.Click("td.shift-category-cell");
			});
		}

		[When(@"I set shift category as '(.*)'")]
		public void WhenISetShiftCategoryAs(string newShiftCat)
		{
			Browser.Interactions.WaitScopeCondition(".edit-shift-category", "vm.shiftCategoriesLoaded", true, () =>
			{
				Browser.Interactions.ClickVisibleOnly(".edit-shift-category .shift-category-selector");
				Browser.Interactions.ClickContaining(".md-select-menu-container md-option .md-text", newShiftCat);
			});
		}

		[When(@"I apply the new shift category")]
		public void WhenIApplyTheNewShiftCategory()
		{
			Browser.Interactions.Click("#applyShiftCategory");
		}

		[Then(@"I should see a shift category named '(.*)'")]
		public void ThenIShouldSeeTheShiftCategoryBecomes(string name)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true, () =>
			{
				Browser.Interactions.AssertFirstContains("td.shift-category-cell", name);
			});
		}

		[Then(@"I should be able to see command check")]
		public void ThenIShouldBeAbleToSeeCommandCheck()
		{
			Browser.Interactions.AssertExists(".teamschedule-command-container .command-check");
		}

		[Then(@"I should be able to see week view toggle button")]
		public void ThenIShouldBeAbleToSeeWeekViewToggleButton()
		{
			Browser.Interactions.AssertAnyContains(".team-schedule .view-changer-wrapper .view-option", "WEEK");
		}

		[Then(@"I should be able to see day view toggle button")]
		public void ThenIShouldBeAbleToSeeDayViewToggleButton()
		{
			Browser.Interactions.AssertAnyContains(".team-schedule .view-changer-wrapper .view-option", "DAY");
		}

		[When(@"I toggle ""(.*)"" view")]
		public void WhenIToggleView(string view)
		{
			Browser.Interactions.ClickContaining(".team-schedule .view-changer-wrapper .view-option", view);
		}

		[Then(@"I should see week view schedule table")]
		public void ThenIShouldSeeWeekViewScheduleTable()
		{
			Browser.Interactions.AssertExists(".weekview-td");
		}
		
		[When(@"I navigate to next week in week view")]
		public void WhenINavigateToNextWeekInWeekView()
		{
			Browser.Interactions.ClickVisibleOnly(".teamschedule-datepicker .mdi-chevron-double-right");
		}

		[Then(@"The dispaly date should be ""(.*)""")]
		public void ThenTheDispalyDateShouldBe(string date)
		{
			Browser.Interactions.AssertInputValue("#teamschedule-datepicker-input", date);
		}

		[When(@"I open teamschedule setting panel")]
		public void WhenIOpenTeamscheduleSettingPanel()
		{
			Browser.Interactions.Click(".team-schedule .settings-container");
		}

		[When(@"I choose not to view '(.*)' validation result")]
		public void WhenIChooseNotToViewValidationResult(string ruleType)
		{
			Browser.Interactions.AssertExists($"div[test-attr={ruleType}] input[type=checkbox]:checked");
			Browser.Interactions.Javascript($"document.querySelector('div[test-attr={ruleType}] input[type=checkbox]').click();");
			Browser.Interactions.AssertExists($"div[test-attr={ruleType}] input[type=checkbox]:not(:checked)");
		}

		[Then(@"I should not see business rule warning")]
		public void ThenIShouldNotSeeBusinessRuleWarning()
		{
			Browser.Interactions.AssertNotExists(".team-schedule", ".team-schedule .warning-icon .mdi-account-alert");
		}

		[When(@"I choose to view '(.*)' validation result")]
		public void WhenIChooseToViewValidationResult(string ruleType)
		{
			Browser.Interactions.AssertExists($"div[test-attr={ ruleType}] input:checked");			
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
