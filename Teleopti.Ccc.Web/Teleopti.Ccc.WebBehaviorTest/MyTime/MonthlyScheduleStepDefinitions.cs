using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

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


    }
}
