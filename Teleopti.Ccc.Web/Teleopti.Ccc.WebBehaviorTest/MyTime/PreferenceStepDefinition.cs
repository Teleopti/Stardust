using System;
using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;


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
		[Then(@"I select shift category '(.*)' as standard preference")]
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
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 3));
		}

		[When(@"I select an editable day with standard preference")]
		public void WhenISelectAnEditableDayWithStandardPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 3));
		}

		[When(@"I click add must have button")]
		public void WhenIClickAddMustHaveButton()
		{
			Browser.Interactions.Click(".dropdown-toggle .glyphicon-heart");
			Browser.Interactions.Click(".dropdown-menu .add-musthave");
		}

		[When(@"I also select an editable day without standard preference")]
		public void WhenISelectAnEditableDayWithoutStandardPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 4));
		}

		[When(@"I select 2 editable day with standard preference")]
		public void WhenISelect2EditableDayWithStandardPreference()
		{
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 3));
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(new DateTime(2014, 5, 4));
		}

		[Then(@"I should see my existing '(.*)' preference")]
		public void ThenIShouldSeeMyExistingDayOffPreference(string preference)
		{
			var date = "2014-05-03";
			var cell = CalendarCells.DateSelector(date);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(cell, preference);
		}

		[Then(@"I should see the workflow control set's standard preferences list with")]
		public void ThenIShouldSeeTheWorkflowControlSetSStandardPreferencesList(Table preferences)
		{
			var items = preferences.CreateSet<PreferenceListItem>();
			items.ForEach(i => Browser.Interactions.AssertFirstContains(".submenu .preference-split-button", i.Preference));
		}

		public class PreferenceListItem
		{
			public string Preference { get; set; }
		}

		[Then(@"I should see the selected standard preference '(.*)' in the split-button")]
		public void ThenIShouldSeeTheSelectedStandardPreferenceInTheSplit_Button(string shiftCategory)
		{
			Browser.Interactions.AssertFirstContains(".submenu .preference-split-button button", shiftCategory);
		}

		[Then(@"I should see the standard preference '(.*)' in the calendar")]
		public void ThenIShouldSeeTheStandardPreferenceInTheCalendar(string preference)
		{
			var date = "2014-05-03";
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector(date), preference);
		}

		[Then(@"I should see the 2 standard preferences '(.*)' in the calendar")]
		public void ThenIShouldSeeThe2StandardPreferencesInTheCalendar(string preference)
		{
			var dateStr1 = "2014-05-03";
			var dateStr2 = "2014-05-04";
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + dateStr1 + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + dateStr2 + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector(dateStr1), preference);
			Browser.Interactions.AssertFirstContains(CalendarCells.DateSelector(dateStr2), preference);
		}

		[Then(@"I should not see the former standard preference in the calendar")]
		public void ThenIShouldNotSeeTheFormerStandardPreferenceInTheCalendar()
		{
			var date = "2014-05-03";
			var data = DataMaker.Data().UserData<StandardPreference>();
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstNotContains(CalendarCells.DateSelector(date), data.Preference);
		}

		[Then(@"I should see the first virtual schedule period overlapping open preference period starting at '(.*)'")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenPreferencePeriod(DateTime date)
		{
			var displayedPeriod = DataMaker.Data().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(new DateOnly(date));
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see the preference period information with open from '(.*)' to '(.*)' and input from '(.*)' to '(.*)'")]
		public void ThenIShouldSeeThePreferencePeriodInformation(DateTime startDate, DateTime endDate, DateTime inputStartDate, DateTime inputEndDate)
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
			var date = "2014-05-03";
			var date2 = "2014-05-04";
			var data1 = DataMaker.Data().UserData<StandardPreference>();
			var data2 = DataMaker.Data().UserData<AnotherStandardPreference>();
			var cell1 = CalendarCells.DateSelector(date);
			var cell2 = CalendarCells.DateSelector(date2);

			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date2 + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstNotContains(cell1, data1.Preference);
			Browser.Interactions.AssertFirstNotContains(cell2, data2.Preference);
		}

		[Then(@"the preference calendar should not be editable")]
		public void ThenThePreferenceCalendarShouldNotBeEditable()
		{
			var amount = Convert.ToInt32(Browser.Interactions.Javascript_IsFlaky("return $('li[data-mytime-date]').length;"));
			for (var i = 0; i < amount; i++)
			{
				var cell = $"li[data-mytime-date]:nth({i})";
				Browser.Interactions.AssertNotExistsUsingJQuery(cell, $"{cell}.ui-selectee");
				Browser.Interactions.ClickUsingJQuery(cell);
				Browser.Interactions.AssertNotExistsUsingJQuery(cell, $"{cell}.ui-selected");
			}
		}

		[Then(@"the preference calendar should be editable")]
		public void ThenThePreferenceCalendarShouldBeEditable()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertExists($"{cell}.editable");
		}

		[Then(@"I should not be able to see preferences link")]
		public void ThenIShouldNotBeAbleToSeePreferencesLink()
		{
			Browser.Interactions.AssertNotExists(".container", "[href*='#PreferenceTab\"]");
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
			var date = "2014-05-03";
			var expected = GetExpectedTimesString(earliest, latest);
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-start-times"), expected);
		}

		[Then(@"I should see the end time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheEndTimeBoundryTo(string earliest, string latest)
		{
			var date = "2014-05-03";
			var expected = GetExpectedTimesString(earliest, latest);
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-end-times"), expected);
		}

		[Then(@"I should see the contract time boundry (\d+) to (\d+)")]
		public void ThenIShouldSeeTheContractTimeBoundryTo(string earliest, string latest)
		{
			var date = "2014-05-03";
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			var culture = DataMaker.Data().MePerson.PermissionInformation.Culture();
			var expected = GetExpectedContractTimesString(earliest, latest, culture);

			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(string.Format("{0} .{1}", cell, "possible-contract-times"), expected);
		}

		[Then(@"I should see that there are no available shifts")]
		public void ThenIShouldSeeThatThereAreNoAvailableShifts()
		{
			var date = "2014-05-03";
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertFirstContains(cell, Resources.NoAvailableShifts);
		}

		[Then(@"I should see my shift for '(.*)'")]
		public void ThenIShouldSeeMyShiftFor(string date)
		{
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");
			Browser.Interactions.AssertAnyContains(cell, "09:00 - 17:00");
		}

		[Then(@"I should see the preference feedback for date ""(.*)""")]
		public void ThenIShouldSeeThePreferenceFeedbackForDate(string date)
		{
			var cell = CalendarCells.DateSelector(date);
			Browser.Interactions.AssertKnockoutContextContains("li[data-mytime-date=\"" + date + "\"] .day-content figure.cover-me", "IsLoading()", "False");

			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-start-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-end-times"));
			Browser.Interactions.AssertNotExists("#Preference-body-inner", string.Format("{0} .{1}:empty", cell, "possible-contract-times"));
		}

		[When(@"I click the delete preference button")]
		public void WhenIClickTheDeletePreferenceButton()
		{
			Browser.Interactions.ClickUsingJQuery("a:has(i.glyphicon-remove)");
		}

		[Then(@"I should see must have number be updated to '(.*)'")]
		public void ThenIShouldSeeMustHaveNumberBeUpdatedTo(int mustHave)
		{ 
			Browser.Interactions.AssertFirstContains(".submenu .musthave-current", mustHave.ToString());
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
			calendarShouldRangeBetween(displayedPeriod.StartDate.Date, displayedPeriod.EndDate.Date);
		}

		private void calendarShouldRangeBetween(DateTime firstDateDisplayed, DateTime lastDateDisplayed)
		{
			Browser.Interactions.AssertNotExists(CalendarCells.DateSelector(firstDateDisplayed), CalendarCells.DateSelector(firstDateDisplayed.AddDays(-1)));
			Browser.Interactions.AssertNotExists(CalendarCells.DateSelector(lastDateDisplayed), CalendarCells.DateSelector(lastDateDisplayed.AddDays(1)));
		}
	}
}
