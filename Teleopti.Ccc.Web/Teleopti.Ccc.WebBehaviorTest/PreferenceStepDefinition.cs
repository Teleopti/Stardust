using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.User;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Find = WatiN.Core.Find;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class PreferenceStepDefinition
	{
		private PreferencePage _page { get { return Pages.Pages.PreferencePage; } }

		[When(@"I click the standard preference split-button")]
		public void WhenIClickTheStandardPreferenceSplit_Button()
		{
			_page.PreferenceButton.ListButton.EventualClick();
			_page.PreferenceButton.Menu.WaitUntilDisplayed();
		}

		[When(@"I change standard preference")]
		[When(@"I select a standard preference")]
		public void WhenIChangeStandardPreference()
		{
			TestMethods.WaitForPreferenceFeedbackToLoad();
			var data = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			_page.SelectPreferenceItemByText(data.ShiftCategory.Description.Name, true);
		}

		[When(@"I try to select a standard preference")]
		public void WhenITryToSelectStandardPreference()
		{
			TestMethods.WaitForPreferenceFeedbackToLoad();
			var data = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			_page.SelectPreferenceItemByText(data.ShiftCategory.Description.Name, false);
		}

		[When(@"I select an editable day without preference")]
		public void WhenISelectAnEditableDayWithoutPreference()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellByClass(date);
		}

		[When(@"I select an editable day with standard preference")]
		public void WhenISelectAnEditableDayWithStandardPreference() 
		{ 
			var data = UserFactory.User().UserData<StandardPreference>();
			_page.SelectCalendarCellByClass(data.Date);
		}

		[When(@"I also select an editable day without standard preference")]
		public void WhenISelectAnEditableDayWithoutStandardPreference()
		{
			var data = UserFactory.User().UserData<StandardPreference>();
			_page.SelectCalendarCellByClass(data.Date.AddDays(1));
		}

		[When(@"I select 2 editable day with standard preference")]
		public void WhenISelect2EditableDayWithStandardPreference()
		{
			var data1 = UserFactory.User().UserData<StandardPreference>();
			var data2 = UserFactory.User().UserData<AnotherStandardPreference>();
			_page.SelectCalendarCellByClass(data1.Date);
			_page.SelectCalendarCellByClass(data2.Date);
		}



		[Then(@"I should see my existing shift category preference")]
		public void ThenIShouldSeeMyExistingShiftCategoryPreference()
		{
			var data = UserFactory.User().UserData<ShiftCategoryPreference>();
			var shiftCategory = data.ShiftCategory.Description.Name;
			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(shiftCategory));
		}

		[Then(@"I should see my existing day off preference")]
		public void ThenIShouldSeeMyExistingDayOffPreference()
		{
			var data = UserFactory.User().UserData<DayOffPreference>();
			var dayOff = data.DayOffTemplate.Description.Name;
			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(dayOff));
		}

		[Then(@"I should see my existing absence preference")]
		public void ThenIShouldSeeMyExistingAbsencePreference()
		{
			var data = UserFactory.User().UserData<AbsencePreference>();
			var absence = data.Absence.Description.Name;

			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(absence));
		}

		[Then(@"I should see the workflow control set's standard preferences list")]
		public void ThenIShouldSeeTheWorkflowControlSetSStandardPreferencesList()
		{
			var data = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			var menu = _page.PreferenceButton.Menu;
			EventualAssert.That(() => menu.Style.Display, Is.EqualTo("block"));
			EventualAssert.That(() => menu.Text, Contains.Substring(data.ShiftCategory.Description.Name));
			EventualAssert.That(() => menu.Text, Contains.Substring(data.DayOffTemplate.Description.Name));
			EventualAssert.That(() => menu.Text, Contains.Substring(data.Absence.Description.Name));
		}

		[Then(@"I should see the selected standard preference in the split-button")]
		public void ThenIShouldSeeTheSelectedStandardPreferenceInTheSplit_Button()
		{
			var data = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>();
			EventualAssert.That(() => _page.PreferenceButton.SetButton.InnerHtml, Contains.Substring(data.ShiftCategory.Description.Name));
		}

		[Then(@"I should see the standard preference in the calendar")]
		public void ThenIShouldSeeTheStandardPreferenceInTheCalendar()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			var shiftCategory = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>().ShiftCategory;
			var cell = _page.CalendarCellForDate(date);
			EventualAssert.WhenElementExists(cell, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
			Navigation.GotoPreference(date);
			EventualAssert.WhenElementExists(cell, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
		}

		[Then(@"I should see the 2 standard preferences in the calendar")]
		public void ThenIShouldSeeThe2StandardPreferencesInTheCalendar()
		{
			var shiftCategory = UserFactory.User().UserData<PreferenceOpenWithAllowedPreferencesWorkflowControlSet>().ShiftCategory;
			var data = UserFactory.User().UserData<StandardPreference>();
			var cell1 = _page.CalendarCellForDate(data.Date);
			var cell2 = _page.CalendarCellForDate(data.Date.AddDays(1));
			EventualAssert.WhenElementExists(cell1, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
			EventualAssert.WhenElementExists(cell1, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
			Navigation.GotoPreference(data.Date);
			EventualAssert.WhenElementExists(cell2, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
			EventualAssert.WhenElementExists(cell2, c => c.Text, Contains.Substring(shiftCategory.Description.Name));
		}

		[Then(@"I should not see the former standard preference in the calendar")]
		public void ThenIShouldNotSeeTheFormerStandardPreferenceInTheCalendar()
		{
			var data = UserFactory.User().UserData<StandardPreference>();
			var cell = _page.CalendarCellForDate(data.Date);
			EventualAssert.That(() => cell.Text, Is.Not.StringContaining(data.Preference));
		}

		[Then(@"I should see the first virtual schedule period overlapping open preference period")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenPreferencePeriod()
		{
			var preferencePeriod = UserFactory.User().UserData<PreferenceOpenNextMonthWorkflowControlSet>().PreferencePeriod;
			var displayedPeriod = UserFactory.User().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(preferencePeriod.StartDate);
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see the preference period information")]
		public void ThenIShouldSeeThePreferencePeriodInformation()
		{
			var data = UserFactory.User().UserData<ExistingWorkflowControlSet>();
			var innerHtml = _page.PreferencePeriod.InnerHtml;
			innerHtml.Should().Contain(data.PreferenceInputPeriod.StartDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.PreferenceInputPeriod.EndDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.PreferencePeriod.StartDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.PreferencePeriod.EndDate.ToShortDateString(UserFactory.User().Culture));
		}

		[Then(@"I should no longer see the 2 standard preferences in the calendar")]
		public void ThenIShouldNoLongerSeeThe2StandardPreferencesInTheCalendar()
		{
			var data1 = UserFactory.User().UserData<StandardPreference>();
			var data2 = UserFactory.User().UserData<AnotherStandardPreference>();
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
			var editableDate = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellForDateByClick(editableDate);
			_page.CalendarCellForDate(editableDate).ClassName.Should().Contain("ui-selected");
		}

		[Then(@"I should not be able to see preferences link")]
		public void ThenIShouldNotBeAbleToSeePreferencesLink()
		{
			var page = Browser.Current.Page<PortalPage>();
			page.Menu.Exists.Should().Be.True();
			page.PreferencesLink.Exists.Should().Be.False();
		}

		[Then(@"I should see the start time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheStartTimeBoundryTo(string earliest, string latest)
		{
			var cell = _page.CalendarCellForDate(DateOnly.Today);
			var div = cell.Div(Find.ByClass("possible-start-times", false));
			TimeSpan earliestTime;
			TimeSpan latestTime;
			TimeHelper.TryParse(earliest, out earliestTime);
			TimeHelper.TryParse(latest, out latestTime);
			var culture = UserFactory.User().Person.PermissionInformation.Culture();
			var expected = TimeHelper.TimeOfDayFromTimeSpan(earliestTime, culture).ToLower() + "-" +
						   TimeHelper.TimeOfDayFromTimeSpan(latestTime, culture).ToLower();
			EventualAssert.That(() => div.InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see the end time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheEndTimeBoundryTo(string earliest, string latest)
		{
			var cell = _page.CalendarCellForDate(DateOnly.Today);
			var div = cell.Div(Find.ByClass("possible-end-times", false));
			TimeSpan earliestTime;
			TimeSpan latestTime;
			TimeHelper.TryParse(earliest, out earliestTime);
			TimeHelper.TryParse(latest, out latestTime);
			var culture = UserFactory.User().Person.PermissionInformation.Culture();
			var expected = TimeHelper.TimeOfDayFromTimeSpan(earliestTime, culture).ToLower() + "-" +
						   TimeHelper.TimeOfDayFromTimeSpan(latestTime, culture).ToLower();
			EventualAssert.That(() => div.InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see the contract time boundry (.*) to (.*)")]
		public void ThenIShouldSeeTheContractTimeBoundryTo(string earliest, string latest)
		{
			var cell = _page.CalendarCellForDate(DateOnly.Today);
			var div = cell.Div(Find.ByClass("possible-contract-times", false));
			TimeSpan earliestTime;
			TimeSpan latestTime;
			TimeHelper.TryParse(earliest, out earliestTime);
			TimeHelper.TryParse(latest, out latestTime);
			var culture = UserFactory.User().Person.PermissionInformation.Culture();
			var expected = TimeHelper.GetLongHourMinuteTimeString(earliestTime, culture).ToLower() + "-" +
						   TimeHelper.GetLongHourMinuteTimeString(latestTime, culture).ToLower();
			EventualAssert.That(() => div.InnerHtml, Is.StringMatching(expected));
		}

		[Then(@"I should see no feedback")]
		public void ThenIShouldSeeNoFeedback()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			var cell = _page.CalendarCellForDate(date);
			var startTimeDiv = cell.Div(Find.ByClass("possible-start-times", false));
			var endTimeDiv = cell.Div(Find.ByClass("possible-end-times", false));
			var contractTimeDiv = cell.Div(Find.ByClass("possible-contract-times", false));

			startTimeDiv.InnerHtml.Should().Be.Null();
			endTimeDiv.InnerHtml.Should().Be.Null();
			contractTimeDiv.InnerHtml.Should().Be.Null();
		}

		[Then(@"I should see that there are no available shifts")]
		public void ThenIShouldSeeThatThereAreNoAvailableShifts()
		{
			var cell = _page.CalendarCellForDate(DateOnly.Today);
			EventualAssert.That(() => cell.InnerHtml, Is.StringContaining(Resources.NoAvailableShifts));
		}

		[Then(@"I should see the preference feedback")]
		public void ThenIShouldSeeThePreferenceFeedback()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			var cell = _page.CalendarCellForDate(date);
			var startTimeDiv = cell.Div(Find.ByClass("possible-start-times", false));
			var endTimeDiv = cell.Div(Find.ByClass("possible-end-times", false));
			var contractTimeDiv = cell.Div(Find.ByClass("possible-contract-times", false));

			startTimeDiv.InnerHtml.Should().Not.Be.NullOrEmpty();
			endTimeDiv.InnerHtml.Should().Not.Be.NullOrEmpty();
			contractTimeDiv.InnerHtml.Should().Not.Be.NullOrEmpty();
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
