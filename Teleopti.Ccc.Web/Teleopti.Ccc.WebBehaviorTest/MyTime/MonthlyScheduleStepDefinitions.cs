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
            Browser.Interactions.Click(".week-schedule-month");
        }
        
        [Then(@"I should end up in month view for '(.*)'")]
        public void ThenIShouldEndUpInMonthViewFor(DateTime date)
        {
            Browser.Interactions.AssertUrlContains(string.Format("Month/{0}/{1}/{2}", date.Year, date.Month.ToString("D2"), date.Day.ToString("D2")));
        }
    }
}
