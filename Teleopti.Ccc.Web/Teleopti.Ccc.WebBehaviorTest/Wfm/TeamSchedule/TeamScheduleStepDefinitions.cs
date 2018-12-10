using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.TestCommon.TestData.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.TeamSchedule
{
	[Binding]
	public sealed class TeamScheduleStepDefinitions
	{
		[When(@"I set schedule date to '(.*)'")]
		public void WhenISetScheduleDateTo(string scheduleDate)
		{
			var propertyValues = new Dictionary<string, string>
			{
				{"vm.scheduleDate", $"new Date('{scheduleDate}')"}
			};

			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.SetScopeValues(".team-schedule", propertyValues);
					Browser.Interactions.AssertScopeValue(".team-schedule", "vm.scheduleDate.toISOString().substr(0,10)",
						scheduleDate);
					Browser.Interactions.InvokeScopeAction(".team-schedule", "vm.onScheduleDateChanged");
					Browser.Interactions.InvokeServiceAction(".team-schedule", "ScenarioTestUtil", "inScenarioTest");
				}
			);
		}

		[When(@"I select a site ""(.*)""")]
		public void WhenISelectASite(string site)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.Click(".orgpicker-wrapper .md-select-value");

					Browser.Interactions.Click(".orgpicker-menu .selection-clear");

					Browser.Interactions.ClickContaining(".repeated-item", $"{site}");

					Browser.Interactions.Click(".orgpicker-menu .selection-done");
				}
			);
		}


		[When(@"I searched schedule with keyword '(.*)'")]
		public void WhenISearchedScheduleWithKeyword(string keyword)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.AssertScopeValue(".team-schedule", "vm.searchEnabled", true);
					Browser.Interactions.FillWith("input.advanced-input", $"{keyword}");
					Browser.Interactions.PressEnter("input.advanced-input");
				});
		}

		[When(@"I should see schedule with absence '(.*)' for '(.*)' displayed")]
		[Then(@"I should see schedule with absence '(.*)' for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleForDisplayed(string absence, string agentName)
		{
			Browser.Interactions.AssertScopeValue(".team-schedule", "vm.scheduleFullyLoaded", true);
			Browser.Interactions.AssertAnyContains(".md-label", agentName);
			Browser.Interactions.AssertExists($".schedule .layer.personAbsence[projection-name='{absence}']");
		}

		[Then(@"I should see schedule with no absence for '(.*)' displayed")]
		public void ThenIShouldSeeScheduleWithNoAbsenceForDisplayed(string agentName)
		{
			Browser.Interactions.AssertScopeValue(".team-schedule", "vm.isLoading", false);
			Browser.Interactions.AssertNotExists(".schedule ", "div.personAbsence");
		}

		[When(@"I selected the person absence for '(.*)'")]
		public void WhenISelectedThePersonAbsenceFor(string agentName)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.ClickContaining(".person-name-text", "John Smith");
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
					Browser.Interactions.ClickContaining(".md-label", agentName);
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
			var values = table.CreateInstance<AddActivityFormInfo>();
			var startTime = '"' + values.StartTime + '"';
			var endTime = '"' + values.EndTime + '"';
			var timeRangeStr = $"{{startTime:{startTime}, endTime:{endTime}}}";
			var selectedDate = $"function(){{return '{values.SelectedDate}';}}";
			var selectedId = idForActivity(values.Activity).ToString();
			var timeRange = new Dictionary<string, string>
					{
						{"vm.selectedDate", selectedDate},
						{"vm.timeRange", timeRangeStr},
						{"vm.selectedActivityId", "'"+ selectedId + "'"}
					};
			Browser.Interactions.SetScopeValues(".add-activity .activity-time-range", timeRange);
		}

		[When(@"I set a new absence as")]
		public void WhenISetANewAbsenceAs(Table table)
		{
			var values = table.CreateInstance<AddAbsenceForm>();
			var selectedDate = $"function(){{return '{values.SelectedDate}';}}";
			var selectedId = idForAbsence(values.Absence).ToString();
			var absenceInput = new Dictionary<string, string>
					{
						{"vm.selectedDate", selectedDate},
						{"vm.selectedAbsenceId", $"'{selectedId}'" },
						{"vm.isFullDayAbsence", $"{(values.FullDay ? "true" : "false")}"}
					};
			Browser.Interactions.SetScopeValues(".add-absence", absenceInput);
			Browser.Interactions.SetScopeValues(".add-absence .start-time teams-time-picker", new Dictionary<string, string> { { "$ctrl.dateTimeObj", $"new Date('{values.StartTime}')" } }, true);
			Browser.Interactions.SetScopeValues(".add-absence .end-time teams-time-picker", new Dictionary<string, string> { { "$ctrl.dateTimeObj", $"new Date('{values.EndTime}')" } }, true);
		}

		[Then(@"I should be able to apply my new activity")]
		public void ThenIShouldBeAbleToApplyMyNewActivity()
		{
			Browser.Interactions.AssertScopeValue("#applyActivity", "newActivityForm.$valid", true);
			Browser.Interactions.AssertScopeValue("#applyActivity", "vm.anyValidAgent()", true);
			Browser.Interactions.AssertExists("#applyActivity.wfm-btn-primary");
		}

		[Then(@"I should be able to apply my new absence")]
		public void ThenIShouldBeAbleToApplyMyNewAbsence()
		{
			Browser.Interactions.AssertExists("#applyAbsence:not([disabled])");
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
			Browser.Interactions.ClickUsingJQuery("#applyActivity");
		}

		[Then(@"I should see agent '(.*)' in the table")]
		public void ThenIShouldSeeAgentInTheTable(string agent)
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true,
				() =>
				{
					Browser.Interactions.ClickContaining(".md-label", agent);
				});
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
			Browser.Interactions.ClickUsingJQuery(".team-schedule-command-confirm-dialog .wfm-btn-primary:not([disabled])");
		}

		[When(@"I selected activity '(.*)'")]
		public void WhenISelectedActivity(string description)
		{
			Browser.Interactions.AssertScopeValue(".team-schedule", "vm.scheduleFullyLoaded", true);
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
				{"vm.moveToTime", '"'+ newStartTime+'"'},
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
			Browser.Interactions.ClickUsingJQuery("#show-warnings");
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
				Browser.Interactions.AssertExists("td.shift-category-cell");
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
			Browser.Interactions.ClickUsingJQuery("#applyShiftCategory");
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

		[When(@"I toggle week view")]
		public void WhenIToggleWeekView()
		{
			Browser.Interactions.ClickContaining(".team-schedule .view-changer-wrapper .view-option", "WEEK");
		}

		[When(@"I toggle day view")]
		public void WhenIToggleDayView()
		{
			Browser.Interactions.ClickContaining(".team-schedule .view-changer-wrapper .view-option", "DAY");
		}
	
		[When(@"I toggle ""(.*)"" view")]
		public void WhenIToggleView(string view)
		{
			Browser.Interactions.ClickContaining(".team-schedule .view-changer-wrapper .view-option", view);
		}

		[Then(@"I should see week view schedule table")]
		public void ThenIShouldSeeWeekViewScheduleTable()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true, () =>
			{
				Browser.Interactions.AssertExists(".weekview-td");
			});
		}

		[When(@"I navigate to next week in week view")]
		public void WhenINavigateToNextWeekInWeekView()
		{
			Browser.Interactions.ClickVisibleOnly(".teamschedule-datepicker .mdi-chevron-double-right");
		}

		[Then(@"The display date should be ""(.*)""")]
		public void ThenTheDisplayDateShouldBe(string date)
		{
			Browser.Interactions.AssertInputValue("#teamschedule-datepicker-input", date);
		}

		[When(@"I open teamschedule setting panel")]
		public void WhenIOpenTeamscheduleSettingPanel()
		{
			Browser.Interactions.Click(".team-schedule .settings-container");
		}

		[When(@"I close teamschedule setting panel")]
		public void WhenICloseTeamscheduleSettingPanel()
		{
			Browser.Interactions.WaitScopeCondition(".team-schedule", "vm.scheduleFullyLoaded", true, () =>
			{
				Browser.Interactions.Click(".team-schedule md-backdrop");
			});

		}

		[When(@"I choose not to view '(.*)' validation result")]
		public void WhenIChooseNotToViewValidationResult(string ruleType)
		{
			Browser.Interactions.AssertExists($"md-checkbox[test-attr={ruleType}][aria-checked=true]");
			Browser.Interactions.Javascript_IsFlaky($"document.querySelector('md-checkbox[test-attr={ruleType}][aria-checked=true]').click();");
			Browser.Interactions.AssertExists($"md-checkbox[test-attr={ruleType}][aria-checked=false]");
		}

		[Then(@"I should not see business rule warning")]
		public void ThenIShouldNotSeeBusinessRuleWarning()
		{
			Browser.Interactions.AssertNotExists(".team-schedule", ".team-schedule .warning-icon .mdi-account-alert");
		}

		[When(@"I choose to view '(.*)' validation result")]
		public void WhenIChooseToViewValidationResult(string ruleType)
		{
			Browser.Interactions.AssertExists($"md-checkbox[test-attr={ ruleType}][aria-checked=true]");
		}

		[Given(@"'(.*)' is in Hawaii time zone")]
		public void GivenIsInHawaiiTimeZone(string person)
		{
			DataMaker.Person(person).Apply(new HawaiiTimeZone());
		}

		[When(@"I click button to search for schedules")]
		[Then(@"I click button to search for schedules")]
		public void WhenIClickButtonToSearchForSchedules()
		{
			Browser.Interactions.Click(".search-icon");
		}

		[When(@"I apply the new absence")]
		public void WhenIApplyTheNewAbsence()
		{
			Browser.Interactions.ClickUsingJQuery("#applyAbsence");
		}

		[Then(@"I should see the start time to move to is '(.*)'")]
		public void ThenIShouldSeeTheStartTimeToMoveToIs(string expected)
		{
			var moveToTime = "vm.moveToTime";
			Browser.Interactions.AssertScopeValue("move-activity", moveToTime, expected, true);
		}


		private static Guid idForActivity(string activityName)
		{
			var activityId = (from a in DataMaker.Data().UserDatasOfType<ActivitySpec>()
							  let activity = a.Activity
							  where activity.Name.Equals(activityName)
							  select activity.Id.GetValueOrDefault()).First();
			return activityId;
		}

		private static Guid idForAbsence(string absenceName)
		{
			var activityId = (from a in DataMaker.Data().UserDatasOfType<AbsenceConfigurable>()
							  let absence = a.Absence
							  where absence.Name.Equals(absenceName)
							  select absence.Id.GetValueOrDefault()).First();
			return activityId;
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

	public class AddAbsenceForm
	{
		public string Absence { get; set; }
		public string SelectedDate { get; set; }
		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool FullDay { get; set; }
	}
}
