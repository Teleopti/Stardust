using System;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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
			Navigation.GotoAvailability();
			_page = Browser.Current.Page<StudentAvailabilityPage>();
		}

		[When(@"I navigate to the student availability page")]
		public void WhenINavigateToTheStudentAvailabilityPage()
		{
			Navigation.GotoAvailability();
		}

		[Then(@"I should see the student availability period information")]
		public void ThenIShouldSeeTheStudentAvailabilityPeriodInformation()
		{
			var data = DataMaker.Data().UserData<ExistingWorkflowControlSet>();
			var innerHtml = _page.StudentAvailabilityPeriod.InnerHtml;
			innerHtml.Should().Contain(data.StudentAvailabilityInputPeriod.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.StudentAvailabilityInputPeriod.EndDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.StudentAvailabilityPeriod.StartDate.ToShortDateString(DataMaker.Data().MyCulture));
			innerHtml.Should().Contain(data.StudentAvailabilityPeriod.EndDate.ToShortDateString(DataMaker.Data().MyCulture));
		}

		[Then(@"the student availabilty calendar should be editable")]
		public void ThenTheCalendarShouldBeEditable()
		{
			if (_page == null)
				ScenarioContext.Current.Pending();
			var editableDate = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			_page.SelectCalendarCellForDateByClick(editableDate);
			_page.CalendarCellForDate(editableDate).ClassName.Should().Contain("ui-selected");
		}

		[Then(@"the student availability calendar should not be editable")]
		public void ThenTheCalendarShouldNotBeEditable()
		{
			_page.CalendarCells.ForEach(cell =>
			                            	{
												cell.ClassName.Should().Not.Contain("ui-selectee");
												_page.SelectCalendarCellByClick(cell);
												cell.ClassName.Should().Not.Contain("ui-selected");
			                            	});
		}

		[Then(@"I should not be able to see student availability link")]
		public void ThenIShouldNotBeAbleToSeeStudentAvailabilityLink()
		{
			Browser.Interactions.AssertNotExists("#signout", "[href*='#StudentAvailabilityTab']");
		}

		[Then(@"I should see my existing student availability")]
		public void ThenIShouldSeeMyExistingStudentAvailability()
		{
			var data = DataMaker.Data().UserData<StudentAvailability>();
			var startTime = TimeHelper.TimeOfDayFromTimeSpan(data.StartTime, DataMaker.Data().MyCulture);
			var endTime = TimeHelper.TimeOfDayFromTimeSpan(data.EndTime, DataMaker.Data().MyCulture);

			EventualAssert.That(()=> _page.CalendarCellForDate(data.Date).InnerHtml, Is.StringContaining(startTime).And.StringContaining(endTime));
		}

		[Then(@"I should see the first virtual schedule period overlapping open student availability period")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenStudentAvailabilityPeriod()
		{
			var studentAvailabilityPeriod = DataMaker.Data().UserData<StudentAvailabilityOpenNextMonthWorkflowControlSet>().StudentAvailabilityPeriod;
			var displayedPeriod = DataMaker.Data().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(studentAvailabilityPeriod.StartDate);
			calendarShouldDisplayPeriod(displayedPeriod);
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
