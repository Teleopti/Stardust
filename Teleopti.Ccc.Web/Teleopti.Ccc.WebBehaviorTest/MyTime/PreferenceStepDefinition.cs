using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferenceStepDefinition
	{
		private PreferencePage _page { get { return Pages.Pages.PreferencePage; } }

		[When(@"I click the standard preference split-button")]
		public void WhenIClickTheStandardPreferenceSplit_Button()
		{
			Browser.Interactions.Javascript("$('#preference-split-button .dropdown-toggle .caret').mousedown();");
		}

		[When(@"I change standard preference")]
		[When(@"I select a standard preference")]
		[When(@"I try to select a standard preference")]
		public void WhenIChangeStandardPreference()
		{
			Browser.Interactions.Click("#preference-split-button .dropdown-toggle");
			var shiftCategory = DataMaker.Data().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>().ShiftCategory;
			Browser.Interactions.ClickContaining("a", shiftCategory.Description.Name);
		}

		[When(@"I select an editable day without preference")]
		public void WhenISelectAnEditableDayWithoutPreference()
		{
			var date = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(date);
		}

		[When(@"I select an editable day with standard preference")]
		public void WhenISelectAnEditableDayWithStandardPreference() 
		{ 
			var data = DataMaker.Data().UserData<StandardPreference>();
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(data.Date);
		}

		[When(@"I also select an editable day without standard preference")]
		public void WhenISelectAnEditableDayWithoutStandardPreference()
		{
			var data = DataMaker.Data().UserData<StandardPreference>();
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(data.Date.AddDays(1));
		}

		[When(@"I select 2 editable day with standard preference")]
		public void WhenISelect2EditableDayWithStandardPreference()
		{
			var data1 = DataMaker.Data().UserData<StandardPreference>();
			var data2 = DataMaker.Data().UserData<AnotherStandardPreference>();
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(data1.Date);
			PreferencesPageStepDefinitions.SelectCalendarCellByClass(data2.Date);
		}

		[Then(@"I should see my existing shift category preference")]
		public void ThenIShouldSeeMyExistingShiftCategoryPreference()
		{
			var data = DataMaker.Data().UserData<ShiftCategoryPreference>();
			var shiftCategory = data.ShiftCategory.Description.Name;
			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(shiftCategory));
		}

		[Then(@"I should see my existing day off preference")]
		public void ThenIShouldSeeMyExistingDayOffPreference()
		{
			var data = DataMaker.Data().UserData<DayOffPreference>();
			var dayOff = data.DayOffTemplate.Description.Name;
			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(dayOff));
		}

		[Then(@"I should see my existing absence preference")]
		public void ThenIShouldSeeMyExistingAbsencePreference()
		{
			var data = DataMaker.Data().UserData<AbsencePreference>();
			var absence = data.Absence.Description.Name;

			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(absence));
		}

		[Then(@"I should see the workflow control set's standard preferences list")]
		public void ThenIShouldSeeTheWorkflowControlSetSStandardPreferencesList()
		{
			var data = DataMaker.Data().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			Browser.Interactions.AssertFirstContains("#preference-split-button", data.ShiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains("#preference-split-button", data.DayOffTemplate.Description.Name);
			Browser.Interactions.AssertFirstContains("#preference-split-button", data.Absence.Description.Name);
		}

		[Then(@"I should see the selected standard preference in the split-button")]
		public void ThenIShouldSeeTheSelectedStandardPreferenceInTheSplit_Button()
		{
			var data = DataMaker.Data().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			Browser.Interactions.AssertFirstContains("#preference-split-button .btn", data.ShiftCategory.Description.Name);
		}

		[Then(@"I should see the standard preference in the calendar")]
		public void ThenIShouldSeeTheStandardPreferenceInTheCalendar()
		{
			var date = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			var shiftCategory = DataMaker.Data().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>().ShiftCategory;
			Browser.Interactions.AssertFirstContains(CalendarCellsPage.DateSelector(date), shiftCategory.Description.Name);
		}

		[Then(@"I should see the 2 standard preferences in the calendar")]
		public void ThenIShouldSeeThe2StandardPreferencesInTheCalendar()
		{
			var shiftCategory = DataMaker.Data().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>().ShiftCategory;
			var data = DataMaker.Data().UserData<StandardPreference>();
			Browser.Interactions.AssertFirstContains(CalendarCellsPage.DateSelector(data.Date), shiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(CalendarCellsPage.DateSelector(data.Date.AddDays(1)), shiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(CalendarCellsPage.DateSelector(data.Date), shiftCategory.Description.Name);
			Browser.Interactions.AssertFirstContains(CalendarCellsPage.DateSelector(data.Date.AddDays(1)), shiftCategory.Description.Name);
		}

		[Then(@"I should not see the former standard preference in the calendar")]
		public void ThenIShouldNotSeeTheFormerStandardPreferenceInTheCalendar()
		{
			var data = DataMaker.Data().UserData<StandardPreference>();
			Browser.Interactions.AssertFirstNotContains(CalendarCellsPage.DateSelector(data.Date), data.Preference);
		}

		[Then(@"I should see the first virtual schedule period overlapping open preference period")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenPreferencePeriod()
		{
			var preferencePeriod = DataMaker.Data().UserData<PreferenceOpenNextMonthWorkflowControlSet>().PreferencePeriod;
			var displayedPeriod = DataMaker.Data().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(preferencePeriod.StartDate);
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see the preference period information")]
		public void ThenIShouldSeeThePreferencePeriodInformation()
		{
			var data = DataMaker.Data().UserData<ExistingWorkflowControlSet>();
			var innerHtml = _page.PreferencePeriodFeedbackView.InnerHtml;
			innerHtml.Should().Contain(data.PreferenceInputPeriod.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.PreferenceInputPeriod.EndDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.PreferencePeriod.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.PreferencePeriod.EndDate.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"I should no longer see the 2 standard preferences in the calendar")]
		public void ThenIShouldNoLongerSeeThe2StandardPreferencesInTheCalendar()
		{
			var data1 = DataMaker.Data().UserData<StandardPreference>();
			var data2 = DataMaker.Data().UserData<AnotherStandardPreference>();
			var cell1 = _page.CalendarCellForDate(data1.Date);
			var cell2 = _page.CalendarCellForDate(data2.Date);
			EventualAssert.That(() => cell1.Text, Is.Not.StringContaining(data1.Preference));
			EventualAssert.That(() => cell2.Text, Is.Not.StringContaining(data2.Preference));
		}

		[Then(@"the preference calendar should not be editable")]
		public void ThenThePreferenceCalendarShouldNotBeEditable()
		{
			_page.CalendarCells.ForEach(cell =>
			{
				cell.ClassName.Should().Not.Contain("ui-selectee");
				_page.SelectCalendarCellByClick(cell);
				cell.ClassName.Should().Not.Contain("ui-selected");
			});
		}

		[Then(@"the preference calendar should be editable")]
		public void ThenThePreferenceCalendarShouldBeEditable()
		{
			var editableDate = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellForDateByClick(editableDate);
			_page.CalendarCellForDate(editableDate).ClassName.Should().Contain("ui-selected");
		}

		[Then(@"I should not be able to see preferences link")]
		public void ThenIShouldNotBeAbleToSeePreferencesLink()
		{
			Browser.Interactions.AssertNotExists(".navbar-inner", "[href*='#PreferenceTab']");
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
			EventualAssert.That(() => _page.CalendarCellDataForDate(DateOnlyForBehaviorTests.TestToday, "possible-start-times").InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see the end time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheEndTimeBoundryTo(string earliest, string latest)
		{
			var expected = GetExpectedTimesString(earliest, latest);

			EventualAssert.That(() => _page.CalendarCellDataForDate(DateOnlyForBehaviorTests.TestToday, "possible-end-times").InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see the contract time boundry (\d+) to (\d+)")]
		public void ThenIShouldSeeTheContractTimeBoundryTo(string earliest, string latest)
		{
			var culture = DataMaker.Data().MePerson.PermissionInformation.Culture();
			var expected = GetExpectedContractTimesString(earliest, latest, culture);

			EventualAssert.That(() => _page.CalendarCellDataForDate(DateOnlyForBehaviorTests.TestToday, "possible-contract-times").InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see that there are no available shifts")]
		public void ThenIShouldSeeThatThereAreNoAvailableShifts()
		{
			var cell = _page.CalendarCellForDate(DateOnlyForBehaviorTests.TestToday);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(Resources.NoAvailableShifts));
		}

		[Then(@"I should see the preference feedback")]
		public void ThenIShouldSeeThePreferenceFeedback()
		{
			var date = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			
			EventualAssert.That(() => _page.CalendarCellDataForDate(date, "possible-start-times").InnerHtml, Is.Not.Null.Or.Empty);
			EventualAssert.That(() => _page.CalendarCellDataForDate(date, "possible-end-times").InnerHtml, Is.Not.Null.Or.Empty);
			EventualAssert.That(() => _page.CalendarCellDataForDate(date, "possible-contract-times").InnerHtml, Is.Not.Null.Or.Empty);
		}

		[When(@"I click the delete preference button")]
		public void WhenIClickTheDeletePreferenceButton()
		{
			Browser.Interactions.Click(".icon-remove");
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
			EventualAssert.That(() => _page.FirstCalendarCellDate, Is.EqualTo(firstDateDisplayed.ToString("yyyy-MM-dd")));
			EventualAssert.That(() => _page.LastCalendarCellDate, Is.EqualTo(lastDateDisplayed.ToString("yyyy-MM-dd")));
		}
	}
}
