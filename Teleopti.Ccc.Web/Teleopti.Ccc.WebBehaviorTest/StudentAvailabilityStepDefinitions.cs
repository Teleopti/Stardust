using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest
{
	[Binding]
	public class StudentAvailabilityStepDefinitions
	{
		private StudentAvailabilityPage _page;


		[Given(@"I am viewing student availability")]
		[When(@"I view student availability")]
		public void WhenIViewStudentAvailability()
		{
			TestControllerMethods.Logon();
			Navigation.GotoStudentAvailability();
			_page = Browser.Current.Page<StudentAvailabilityPage>();
		}

		[When(@"I select an editable day without student availability")]
		public void WhenISelectAnEditableDayWithoutStudentAvailability()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellForDateByClick(date);
		}

		[When(@"I click the edit button")]
		public void WhenIClickEditButton()
		{
			_page.EditButton.WaitUntil(b => b.Enabled, 5);
			_page.EditButton.Click();
		}

		[When(@"I click the cancel button")]
		public void WhenIClickTheCancelButton()
		{
			_page.CancelButton.WaitUntil(b => b.Enabled, 5);
			_page.CancelButton.Click();
		}

		[When(@"I input student availability values")]
		public void WhenIInputStudentAvailabilityValues()
		{
			_page.StartTimeTextField.Value = "05:00";
			_page.EndTimeTextField.Value = "14:00";
			_page.NextDay.Checked = false;
		}

		[When(@"I input invalid student availability values")]
		public void WhenIInvalidInputStudentAvailabilityValues()
		{
			_page.StartTimeTextField.Value = "not-a";
			_page.EndTimeTextField.Value = "-time";
			_page.NextDay.Checked = false;
		}

		[When(@"I input student availability values with end time on next day")]
		public void WhenIInputStudentAvailabilityValuesWithEndTimeOnNextDay()
		{
			_page.StartTimeTextField.Value = "14:00";
			_page.EndTimeTextField.Value = "05:00";
			_page.NextDay.Checked = true;
		}

		[When(@"I navigate to the student availability page")]
		public void WhenINavigateToTheStudentAvailabilityPage()
		{
			Navigation.GotoStudentAvailability();
		}

		[When(@"I select the day with student availability")]
		public void WhenISelectTheDayWithStudentAvailability()
		{
			var date = UserFactory.User().UserData<StudentAvailability>().Date;
			_page.SelectCalendarCellForDateByClick(date);
		}

		[When(@"I select a day with student availability")]
		public void WhenISelectADayWithStudentAvailability()
		{
			var date = UserFactory.User().UserData<StudentAvailability>().Date;
			_page.SelectCalendarCellForDateByClick(date);
		}

		[Then(@"I should see the student availability values in the input form")]
		public void ThenIShouldSeeTheStudentAvailabilityValuesInTheInputForm()
		{
			var studentAvailability = UserFactory.User().UserData<StudentAvailability>();
			Assert.That(() => TimeSpan.Parse(_page.StartTimeTextField.Text), Is.EqualTo(studentAvailability.StartTime).After(5000, 10));
			Assert.That(() => TimeSpan.Parse(_page.EndTimeTextField.Text), Is.EqualTo(studentAvailability.EndTime).After(5000, 10));
		}

		[Then(@"I should see the student availability period information")]
		public void ThenIShouldSeeTheStudentAvailabilityPeriodInformation()
		{
			var data = UserFactory.User().UserData<ExistingWorkflowControlSet>();
			var innerHtml = _page.StudentAvailabilityPeriod.InnerHtml;
			innerHtml.Should().Contain(data.StudentAvailabilityInputPeriod.StartDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.StudentAvailabilityInputPeriod.EndDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.StudentAvailabilityPeriod.StartDate.ToShortDateString(UserFactory.User().Culture));
			innerHtml.Should().Contain(data.StudentAvailabilityPeriod.EndDate.ToShortDateString(UserFactory.User().Culture));
		}

		[Then(@"I should see the new student availability values in the calendar")]
		public void ThenIShouldSeeTheNewStudentAvailabilityValuesInTheCalendar()
		{
			var date = UserFactory.User().UserData<StudentAvailability>().Date;
			cellShouldContainInputValues(date);
			Navigation.GotoStudentAvailability(date);
			cellShouldContainInputValues(date);
		}

		[Then(@"I should see the student availability in the calendar")]
		public void ThenIShouldSeeTheStudentAvailabilityInTheCalendar()
		{
			var date = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.CalendarCellForDate(date).WaitUntil(p => p.ClassName.Contains("unvalidated"), 500);
			cellShouldContainInputValues(date);
			Navigation.GotoStudentAvailability(date);
			cellShouldContainInputValues(date);
		}

		[Then(@"the student availability values in the calendar should disappear")]
		public void ThenTheStudentAvailabilityValuesInTheCalendarShouldDisappear()
		{
			var data = UserFactory.User().UserData<StudentAvailability>();
			var startTime = TimeHelper.TimeOfDayFromTimeSpan(data.StartTime, UserFactory.User().Culture);
			var endTime = TimeHelper.TimeOfDayFromTimeSpan(data.EndTime, UserFactory.User().Culture);

			var cell = _page.CalendarCellForDate(data.Date);
			Assert.That(() => cell.InnerHtml, Is.Not.ContainsSubstring(startTime).After(5000, 10));
			Assert.That(() => cell.InnerHtml, Is.Not.ContainsSubstring(endTime).After(5000, 10));
		}

		[Then(@"the student availabilty calendar should be editable")]
		public void ThenTheCalendarShouldBeEditable()
		{
			if (_page == null)
				ScenarioContext.Current.Pending();
			var editableDate = UserFactory.User().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellForDateByClick(editableDate);
			_page.CalendarCellForDate(editableDate).ClassName.Should().Contain("ui-selected");
			_page.EditButton.Click();
			Assert.That(() => _page.InputPanel.Style.Display, Is.Not.EqualTo("none").After(5000, 10));
		}

		[Then(@"the student availability calendar should not be editable")]
		public void ThenTheCalendarShouldNotBeEditable()
		{
			_page.CalendarCells.ForEach(cell =>
			                            	{
												cell.ClassName.Should().Not.Contain("ui-selectee");
												_page.SelectCalendarCellByClick(cell);
												cell.ClassName.Should().Not.Contain("ui-selected");
												_page.EditButton.Enabled.Should().Be.False();
			                            	});
		}

		[Then(@"the calendar is disabled")]
		public void ThenTheCalendarIsDisabled()
		{
			Assert.That(() => Browser.Current.Div("modal-disable").Style.Display, Is.Not.EqualTo("none").After(5000, 10));
		}

		[Then(@"I should not see the student availability values")]
		public void ThenIShouldNotSeeTheStudentAvailabilityValues()
		{
			Assert.That(() => _page.InputPanel.Style.Display, Is.EqualTo("none").After(5000, 10));
		}

		[Then(@"I should not be able to see student availability link")]
		public void ThenIShouldNotBeAbleToSeeStudentAvailabilityLink()
		{
			var page = Browser.Current.Page<PortalPage>();
			page.Menu.Exists.Should().Be.True();
			page.StudentAvailabilityLink.Exists.Should().Be.False();
		}

		[Then(@"I should see my existing student availability")]
		public void ThenIShouldSeeMyExistingStudentAvailability()
		{
			var data = UserFactory.User().UserData<StudentAvailability>();
			var startTime = TimeHelper.TimeOfDayFromTimeSpan(data.StartTime, UserFactory.User().Culture);
			var endTime = TimeHelper.TimeOfDayFromTimeSpan(data.EndTime, UserFactory.User().Culture);

			var innerHtml = _page.CalendarCellForDate(data.Date).InnerHtml;
			innerHtml.Should().Contain(startTime);
			innerHtml.Should().Contain(endTime);
		}

		[Then(@"I should see the first virtual schedule period overlapping open student availability period")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenStudentAvailabilityPeriod()
		{
			var studentAvailabilityPeriod = UserFactory.User().UserData<StudentAvailabilityOpenNextMonthWorkflowControlSet>().StudentAvailabilityPeriod;
			var displayedPeriod = UserFactory.User().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(studentAvailabilityPeriod.StartDate);
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see a message saying I have given an invalid time value")]
		public void ThenIShouldSeeAMessageSayingIHaveGivenAnInvalidTimeValue()
		{
			EventualAssert.That(() => _page.ValidationErrorText.Exists, Is.True);
			EventualAssert.That(() => _page.ValidationErrorText.InnerHtml, Is.StringContaining(string.Format(Resources.InvalidTimeValue, "not-a")));
			EventualAssert.That(() => _page.ValidationErrorText.InnerHtml, Is.StringContaining(string.Format(Resources.InvalidTimeValue, "-time")));
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

		private void cellShouldContainInputValues(DateTime date)
		{
			EventualAssert.That(() => _page.CalendarCellForDate(date).InnerHtml, Is.StringContaining("05:00"));
			EventualAssert.That(() => _page.CalendarCellForDate(date).InnerHtml, Is.StringContaining("14:00"));
		}

	}
}
