using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class MonthlyScheduleStepDefinitions
    {
        [When(@"I choose to go to month view")]
        public void WhenIChooseToGoToMonthView()
        {
            Browser.Interactions.Click("#week-schedule-month");
        }
        
        [Then(@"I should end up in month view for '(.*)'")]
        public void ThenIShouldEndUpInMonthViewFor(DateTime date)
        {
            Browser.Interactions.AssertUrlContains(string.Format("Month/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
        }

        [Then(@"I should see '(.*)' as the first day")]
        public void ThenIShouldSeeAsTheFirstDay(DateTime date)
        {
            DateTime dateBefore = date.AddDays(-1);
            Browser.Interactions.AssertNotExists(string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", date),
                                                 string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", dateBefore));
        }

        [Then(@"I should see '(.*)' as the last day")]
        public void ThenIShouldSeeAsTheLastDay(DateTime date)
        {
            DateTime dateAfter = date.AddDays(1);
            Browser.Interactions.AssertNotExists(string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", date),
                                                 string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", dateAfter));
        }

        [Then(@"I should see the day '(.*)' is not part of current month")]
        public void ThenIShouldSeeTheDayIsNotPartOfCurrentMonth(DateTime date)
        {
            Browser.Interactions.AssertExists(string.Format(".cal-day-outmonth span[data-cal-date='{0:yyyy-MM-dd}']", date));
        }

        [Then(@"I should see an indication implying I should work on '(.*)'")]
        public void ThenIShouldSeeAnIndicationImplyingIShouldWorkOn(DateTime date)
        {
            Browser.Interactions.AssertExists(string.Format(".working-day span[data-cal-date='{0:yyyy-MM-dd}']", date));
        }

        [Then(@"I should see an indication implying I should not work on '(.*)'")]
        public void ThenIShouldSeeAnIndicationImplyingIShouldNotWorkOn(DateTime date)
        {
            Browser.Interactions.AssertExists(string.Format(".not-working-day span[data-cal-date='{0:yyyy-MM-dd}']", date));
        }

        [Then(@"I should not see any indication for day '(.*)'")]
        public void ThenIShouldNotSeeAnyIndicationForDay(DateTime date)
        {
            Browser.Interactions.AssertNotExists(string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", date),
                string.Format(".not-working-day span[data-cal-date='{0:yyyy-MM-dd}']", date));
            Browser.Interactions.AssertNotExists(string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", date), 
                string.Format(".working-day span[data-cal-date='{0:yyyy-MM-dd}']", date));
        }

        [Then(@"I should end up in month view for current month")]
        public void ThenIShouldEndUpInMonthViewForCurrentMonth()
        {
            Browser.Interactions.AssertExists(string.Format("span[data-cal-date='{0:yyyy-MM-dd}']", DateTime.Today));
        }

        [Then(@"I should see '(.*)' as the first day of week label")]
        public void ThenIShouldSeeAsTheFirstDayOfWeekLabel(string dayOfWeek)
        {
            Browser.Interactions.AssertFirstContainsUsingJQuery(".weekday-name", dayOfWeek);
            //Browser.Interactions.AssertFirstContains(".weekday-name", dayOfWeek);
        }

        [Given(@"I am using a device with narrow view")]
        public void GivenIAmUsingADeviceWithNarrowView()
        {
            Browser.Interactions.SetWidth(480);
        }

        [When(@"I choose the day '(.*)'")]
        public void WhenIChooseTheDay(DateTime date)
        {
            Browser.Interactions.Click(string.Format(".cal-month-day span[data-cal-date='{0:yyyy-MM-dd}']", date));
        }

        [Then(@"I should end up in week view for '(.*)'")]
        public void ThenIShouldEndUpInWeekViewFor(DateTime date)
        {
            Browser.Interactions.AssertUrlContains(string.Format("Week/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
        }

        [When(@"I choose to go to next month")]
        public void WhenIChooseToGoToNextMonth()
        {
            Browser.Interactions.Click(".glyphicon-arrow-right");
        }

        [When(@"I choose to go to previous month")]
        public void WhenIChooseToGoToPreviousMonth()
        {
            Browser.Interactions.Click(".glyphicon-arrow-left");
        }


        [Then(@"I should see the month name being '(.*)'")]
        public void ThenIShouldSeeTheMonthName(string name)
        {
            Browser.Interactions.AssertInputValueUsingJQuery("input",name);
        }

        [When(@"I select the month '(.*)' in the calendar")]
        public void WhenISelectTheMonthInTheCalendar(string monthName)
        {
            Browser.Interactions.Click(".glyphicon-calendar");
            string selector = string.Format(".datepicker-months .month:contains('{0}')",monthName);
            Browser.Interactions.AssertVisibleUsingJQuery(selector);
            Browser.Interactions.ClickUsingJQuery(selector);
        }

        [Then(@"I should not be able to access month view")]
        public void ThenIShouldNotBeAbleToAccessMonthView()
        {
            Browser.Interactions.AssertNotExists("#week-schedule-today", "#week-schedule-month");
        }
    }
}
