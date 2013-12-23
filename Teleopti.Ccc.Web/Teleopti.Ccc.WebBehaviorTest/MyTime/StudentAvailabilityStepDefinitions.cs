﻿using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class StudentAvailabilityStepDefinitions
	{
		[Given(@"I am viewing student availability")]
		[When(@"I view student availability")]
		public void WhenIViewStudentAvailability()
		{
			TestControllerMethods.Logon();
			Navigation.GotoAvailability();
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
			var cultureInfo = DataMaker.Data().MyCulture;

			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",data.StudentAvailabilityInputPeriod.StartDate.ToShortDateString(cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",data.StudentAvailabilityInputPeriod.EndDate.ToShortDateString(cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",data.StudentAvailabilityPeriod.StartDate.ToShortDateString(cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",data.StudentAvailabilityPeriod.EndDate.ToShortDateString(cultureInfo));
		}

		[Then(@"the student availabilty calendar should be editable")]
		public void ThenTheCalendarShouldBeEditable()
		{
			var editableDate = DataMaker.Data().UserData<SchedulePeriod>().FirstDateInVirtualSchedulePeriod();
			var cell = CalendarCells.DateSelector(editableDate);
			Browser.Interactions.ClickUsingJQuery(cell);
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("{0}.{1}",cell,"ui-selected"));
		}

		[Then(@"the student availability calendar should not be editable")]
		public void ThenTheCalendarShouldNotBeEditable()
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

			var cell = CalendarCells.DateSelector(data.Date);
			Browser.Interactions.AssertFirstContainsUsingJQuery(cell,startTime);
			Browser.Interactions.AssertFirstContainsUsingJQuery(cell,endTime);
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
			Browser.Interactions.AssertNotExistsUsingJQuery(CalendarCells.DateSelector(firstDateDisplayed), CalendarCells.DateSelector(firstDateDisplayed.AddDays(-1)));
			Browser.Interactions.AssertNotExistsUsingJQuery(CalendarCells.DateSelector(lastDateDisplayed), CalendarCells.DateSelector(lastDateDisplayed.AddDays(1)));
		}
	}
}
