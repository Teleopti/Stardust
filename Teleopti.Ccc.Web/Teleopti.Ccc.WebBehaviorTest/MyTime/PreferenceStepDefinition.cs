using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferenceStepDefinition
	{
		[When(@"I click the standard preference split-button")]
		public void WhenIClickTheStandardPreferenceSplit_Button()
		{
			Browser.Interactions.ClickUsingJQuery(".preference-split-button .dropdown-toggle");
		}

		[When(@"I change standard preference to shift category '(.*)'")]
		[When(@"I select shift category '(.*)' as standard preference")]
		[When(@"I try to select shift category '(.*)' standard preference")]
		public void WhenIChangeStandardPreference(string shiftCategory)
		{
			Browser.Interactions.ClickUsingJQuery(".preference-split-button .dropdown-toggle");
			Browser.Interactions.ClickUsingJQuery(string.Format("a:contains('{0}')", shiftCategory));
		}

		[When(@"I select an editable day without preference")]
		public void WhenISelectAnEditableDayWithoutPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014,5,3));
		}

		[When(@"I select an editable day with standard preference")]
		public void WhenISelectAnEditableDayWithStandardPreference() 
		{ 
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014,5,3));
		}

		[When(@"I also select an editable day without standard preference")]
		public void WhenISelectAnEditableDayWithoutStandardPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014,5,4));
		}

		[When(@"I select 2 editable day with standard preference")]
		public void WhenISelect2EditableDayWithStandardPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 3));
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 4));
		}

		[Then(@"I should see my existing shift category preference")]
		public void ThenIShouldSeeMyExistingShiftCategoryPreference()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(cell, TestData.ShiftCategory.Description.Name);
		}

		[Then(@"I should see my existing day off preference")]
		public void ThenIShouldSeeMyExistingDayOffPreference()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(cell, TestData.DayOffTemplate.Description.Name);
		}

		[Then(@"I should see my existing absence preference")]
		public void ThenIShouldSeeMyExistingAbsencePreference()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(cell,TestData.Absence.Description.Name);
		}

		[Then(@"I should see the workflow control set's standard preferences list")]
		public void ThenIShouldSeeTheWorkflowControlSetSStandardPreferencesList()
		{
			Browser.Interactions.AssertFirstContains(".hidden-sm .preference-split-button", TestData.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(".hidden-sm .preference-split-button", TestData.DayOffTemplate.Description.Name);
			Browser.Interactions.AssertFirstContains(".hidden-sm .preference-split-button", TestData.Absence.Description.Name);
		}

		[Then(@"I should see the selected standard preference in the split-button")]
		public void ThenIShouldSeeTheSelectedStandardPreferenceInTheSplit_Button()
		{
			Browser.Interactions.AssertFirstContains(".hidden-sm .preference-split-button button", TestData.ShiftCategory.Description.Name);
		}

		[Then(@"I should see the standard preference in the calendar")]
		public void ThenIShouldSeeTheStandardPreferenceInTheCalendar()
		{
			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector("2014-05-03"), TestData.ShiftCategory.Description.Name);
		}

		[Then(@"I should see the 2 standard preferences in the calendar")]
		public void ThenIShouldSeeThe2StandardPreferencesInTheCalendar()
		{
			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector("2014-05-03"), TestData.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector("2014-05-04"), TestData.ShiftCategory.Description.Name);
		}

		[Then(@"I should not see the former standard preference in the calendar")]
		public void ThenIShouldNotSeeTheFormerStandardPreferenceInTheCalendar()
		{
			var data = DataMaker.Data().UserData<StandardPreference>();
			Browser.Interactions.AssertFirstNotContains(CalendarCells.DateSelector("2014-05-03"), data.Preference);
		}

		[Then(@"I should see the first virtual schedule period overlapping open preference period starting at '(.*)'")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenPreferencePeriod(DateTime date)
		{
			var displayedPeriod = DataMaker.Data().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(new DateOnly(date));
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see the preference period information with open from '(.*)' to '(.*)' and input from '(.*)' to '(.*)'")]
		public void ThenIShouldSeeThePreferencePeriodInformation(DateTime startDate,DateTime endDate,DateTime inputStartDate,DateTime inputEndDate)
		{
			var cultureInfo = DataMaker.Data().MyCulture;

			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", inputStartDate.ToString("d", cultureInfo));
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", inputEndDate.ToString("d", cultureInfo));
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", startDate.ToString("d", cultureInfo));
			Browser.Interactions.AssertAnyContains("#Preference-period-feedback-view", endDate.ToString("d", cultureInfo));
		}

		[Then(@"I should no longer see the 2 standard preferences in the calendar")]
		public void ThenIShouldNoLongerSeeThe2StandardPreferencesInTheCalendar()
		{
			var data1 = DataMaker.Data().UserData<StandardPreference>();
			var data2 = DataMaker.Data().UserData<AnotherStandardPreference>();
			var cell1 = CalendarCells.DateSelector("2014-05-03");
			var cell2 = CalendarCells.DateSelector("2014-05-04");
			Browser.Interactions.AssertFirstNotContains(cell1,data1.Preference);
			Browser.Interactions.AssertFirstNotContains(cell2,data2.Preference);
		}

		[Then(@"the preference calendar should not be editable")]
		public void ThenThePreferenceCalendarShouldNotBeEditable()
		{
			var amount = Convert.ToInt32(Browser.Interactions.Javascript("return $('li[data-mytime-date]').length;"));
			for (int i = 0; i < amount; i++)
			{
				var cell = string.Format("li[data-mytime-date]:nth({0})", i);
				Browser.Interactions.AssertNotExistsUsingJQuery(cell, string.Format("{0}.{1}", cell, "ui-selectee"));
				Browser.Interactions.ClickUsingJQuery(cell);
				Browser.Interactions.AssertNotExistsUsingJQuery(cell, string.Format("{0}.{1}", cell, "ui-selected"));
			}
		}

		[Then(@"the preference calendar should be editable")]
		public void ThenThePreferenceCalendarShouldBeEditable()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.Click(cell);
			Browser.Interactions.AssertExists(string.Format("{0}.{1}", cell, "ui-selected"));
		}

		[Then(@"I should not be able to see preferences link")]
		public void ThenIShouldNotBeAbleToSeePreferencesLink()
		{
			Browser.Interactions.AssertNotExists(".container", "[href*='#PreferenceTab']");
		}

		[Then(@"I should see the contract time of '(.*)' hours")]
		public void ThenIShouldSeeTheContractTimeOfHours(string hours)
		{
			Browser.Interactions.AssertFirstContains(".schedule-contract-time", hours);
		}

		[Then(@"I should see the absence contract time of '(.*)' hours")]
		public void ThenIShouldSeeTheAbsenceContractTimeOfHours(string hours)
		{
			Browser.Interactions.AssertExists(".absence-contract-time");
		}

		[Then(@"I should see the start time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheStartTimeBoundryTo(string earliest, string latest)
		{
			var expected = GetExpectedTimesString(earliest, latest);
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-start-times"), expected);
		}

		[Then(@"I should see the end time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheEndTimeBoundryTo(string earliest, string latest)
		{
			var expected = GetExpectedTimesString(earliest, latest);
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-end-times"), expected);
		}

		[Then(@"I should see the contract time boundry (\d+) to (\d+)")]
		public void ThenIShouldSeeTheContractTimeBoundryTo(string earliest, string latest)
		{
			var culture = DataMaker.Data().MePerson.PermissionInformation.Culture();
			var expected = GetExpectedContractTimesString(earliest, latest, culture);

			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-contract-times"), expected);
		}

		[Then(@"I should see that there are no available shifts")]
		public void ThenIShouldSeeThatThereAreNoAvailableShifts()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContains(cell, Resources.NoAvailableShifts);
		}

		[Then(@"I should see the preference feedback")]
		public void ThenIShouldSeeThePreferenceFeedback()
		{
			var date = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();

			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-start-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-end-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-contract-times"));
		}

		[When(@"I click the delete preference button")]
		public void WhenIClickTheDeletePreferenceButton()
		{
			Browser.Interactions.ClickUsingJQuery("a:has(i.glyphicon-remove)");
		}

		private static string GetExpectedContractTimesString(string earliest, string latest, CultureInfo culture)
		{
			TimeSpan earliestTime;
			TimeSpan latestTime;
			TimeHelper.TryParse(earliest, out earliestTime);
			TimeHelper.TryParse(latest, out latestTime);
			return TimeHelper.GetLongHourMinuteTimeString(earliestTime, culture).ToLower()
				   + "-" +
				   TimeHelper.GetLongHourMinuteTimeString(latestTime, culture).ToLower();
		}

		private static string GetExpectedTimesString(string earliest, string latest)
		{
			TimeSpan earliestTime;
			TimeSpan latestTime;
			TimeHelper.TryParse(earliest, out earliestTime);
			TimeHelper.TryParse(latest, out latestTime);
			var culture = DataMaker.Data().MePerson.PermissionInformation.Culture();
			var expected = TimeHelper.TimeOfDayFromTimeSpan(earliestTime, culture).ToLower()
			               + "-" +
			               TimeHelper.TimeOfDayFromTimeSpan(latestTime, culture).ToLower();
			return expected;
		}

		private void calendarShouldDisplayPeriod(DateOnlyPeriod displayedPeriod)
		{
			calendarShouldRangeBetween(displayedPeriod.StartDate, displayedPeriod.EndDate);
		}

		private void calendarShouldRangeBetween(DateTime firstDateDisplayed, DateTime lastDateDisplayed)
		{
			Browser.Interactions.AssertNotExists(CalendarCells.DateSelector(firstDateDisplayed), CalendarCells.DateSelector(firstDateDisplayed.AddDays(-1)));
			Browser.Interactions.AssertNotExists(CalendarCells.DateSelector(lastDateDisplayed), CalendarCells.DateSelector(lastDateDisplayed.AddDays(1)));
		}
	}
}
