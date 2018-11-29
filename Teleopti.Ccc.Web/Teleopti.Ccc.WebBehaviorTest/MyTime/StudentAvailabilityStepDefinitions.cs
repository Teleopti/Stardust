using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;


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

		[Given(@"I am viewing student availability for '(.*)'")]
		[When(@"I view student availability for '(.*)'")]
		public void WhenIViewStudentAvailabilityFor(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoAvailability(date);
		}

		[When(@"I navigate to the student availability page")]
		public void WhenINavigateToTheStudentAvailabilityPage()
		{
			Navigation.GotoAvailability();
		}

		[Then(@"I should see the student availability period information with period '(.*)' to '(.*)', and input period '(.*)' to '(.*)'")]
		public void ThenIShouldSeeTheStudentAvailabilityPeriodInformation(DateTime startDate,DateTime endDate,DateTime inputStartDate,DateTime inputEndDate)
		{
			var cultureInfo = DataMaker.Data().MyCulture;

			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",inputStartDate.ToString("d",cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",inputEndDate.ToString("d",cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",startDate.ToString("d",cultureInfo));
			Browser.Interactions.AssertFirstContainsUsingJQuery("#StudentAvailability-period",endDate.ToString("d",cultureInfo));
		}

		[Then(@"the student availabilty calendar should be editable")]
		public void ThenTheCalendarShouldBeEditable()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.Click(cell);
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("{0}.{1}",cell,"ui-selected"));
		}

		[Then(@"the student availability calendar should not be editable")]
		public void ThenTheCalendarShouldNotBeEditable()
		{
			var amount = Convert.ToInt32(Browser.Interactions.Javascript_IsFlaky("return $('li[data-mytime-date]').length;"));
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
			Browser.Interactions.AssertNotExists("#regional-settings", "[href*='#StudentAvailabilityTab']");
		}

		[Then(@"I should see my existing student availability")]
		public void ThenIShouldSeeMyExistingStudentAvailability()
		{
			var cell = CalendarCells.DateSelector("2014-05-03");
			Browser.Interactions.AssertFirstContainsUsingJQuery(cell,"07:00");
			Browser.Interactions.AssertFirstContainsUsingJQuery(cell,"18:00");
		}

		[Then(@"I should see the first virtual schedule period overlapping open student availability period starting at '(.*)'")]
		public void ThenIShouldSeeTheFirstVirtualSchedulePeriodOverlappingOpenStudentAvailabilityPeriod(DateTime date)
		{
			var displayedPeriod = DataMaker.Data().UserData<SchedulePeriod>().DisplayedVirtualSchedulePeriodForDate(new DateOnly(date));
			calendarShouldDisplayPeriod(displayedPeriod);
		}

		[Then(@"I should see I am not available for '(.*)'")]
		public void ThenIShouldSeeIAmNotAvailableFor(string date)
		{
			var seletor = string.Format("li[data-mytime-date='{0}'] .canAddAvailibility", date);
			Browser.Interactions.AssertExists(seletor);
		}

		[Then(@"I should see there is no valid shift for my availability on '(.*)'")]
		public void ThenIShouldSeeThereIsNoValidShiftForMyAvailabilityOn(string date)
		{
			var seletor = string.Format("li[data-mytime-date='{0}'] .feedback-error:contains('{1}')", date,
				Resources.NoAvailableShifts);
			Browser.Interactions.AssertExistsUsingJQuery(seletor);
		}

		[Then(@"I should see valid shift for my availability on '(.*)'")]
		public void ThenIShouldSeeValidShiftForMyAvailabilityOn(string date)
		{
			var startTimeSeletor = string.Format("li[data-mytime-date='{0}'] .possible-start-times", date);
			Browser.Interactions.AssertExists(startTimeSeletor);

			var endTimeSeletor = string.Format("li[data-mytime-date='{0}'] .possible-end-times", date);
			Browser.Interactions.AssertExists(endTimeSeletor);

			var contractTimeSeletor = string.Format("li[data-mytime-date='{0}'] .possible-contract-times", date);
			Browser.Interactions.AssertExists(contractTimeSeletor);
		}

		[Then(@"I should see the period feedback")]
		public void ThenIShouldSeeThePeriodFeedback()
		{
			const string periodfeedback = ".alert-student-availability:contains('Du borde arbeta minst')";
			Browser.Interactions.AssertExistsUsingJQuery(periodfeedback);
		}

		private void calendarShouldDisplayPeriod(DateOnlyPeriod displayedPeriod)
		{
			calendarShouldRangeBetween(displayedPeriod.StartDate, displayedPeriod.EndDate);
		}

		private void calendarShouldRangeBetween(DateOnly firstDateDisplayed, DateOnly lastDateDisplayed)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(CalendarCells.DateSelector(firstDateDisplayed.Date), CalendarCells.DateSelector(firstDateDisplayed.AddDays(-1).Date));
			Browser.Interactions.AssertNotExistsUsingJQuery(CalendarCells.DateSelector(lastDateDisplayed.Date), CalendarCells.DateSelector(lastDateDisplayed.AddDays(1).Date));
		}
	}
}
