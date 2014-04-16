using System;
using System.Globalization;
using System.Xml;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages;
using Teleopti.Interfaces.Domain;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class WeekScheduleOnMobileStepDefinitions
    {
        [Then(@"I should see my mobile week schedule for date '(.*)'")]
        public void ThenIShouldSeeMyMobileWeekScheduleForDate(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I click the desktop link")]
        public void WhenIClickTheDesktopLink()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see mobile view of the week with for date '(.*)'")]
        public void ThenIShouldSeeMobileViewOfTheWeekWithForDate(DateTime date)
        {
            AssertShowingWeekForDay(DateHelper.GetFirstDateInWeek(date.Date, DataMaker.Data().MyCulture));
        }

        [Then(@"I should see the shift with")]
        public void ThenIShouldSeeTheShiftWith(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see the day off on '(.*)'")]
        public void ThenIShouldSeeTheDayOffOn(string date)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see the absence on date '(.*)'")]
        public void ThenIShouldSeeTheAbsenceOnDate(string date)
        {
            ScenarioContext.Current.Pending();
        }

        [Scope(Feature = "View week schedule on mobile")]
        [Then(@"I should not see a shift on date '(.*)'")]
        public void ThenIShouldNotSeeAShiftOnDate(DateTime date)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see the absence with")]
        public void ThenIShouldSeeTheAbsenceWith(Table table)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should not see any shift for day '(.*)'")]
        public void ThenIShouldNotSeeAnyShiftForDay(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see a day name being '(.*)'")]
        public void ThenIShouldSeeADayNameBeing(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see '(.*)' as the first day")]
        public void ThenIShouldSeeAsTheFirstDay(DateTime date)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see '(.*)' as the last day")]
        public void ThenIShouldSeeAsTheLastDay(DateTime date)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see '(.*)' as the first day of week label")]
        public void ThenIShouldSeeAsTheFirstDayOfWeekLabel(string dayOfWeek)
        {
            ScenarioContext.Current.Pending();
        }
        [Given(@"I view my mobile week schedule for date '(.*)'")]
        public void GivenIViewMyMobileWeekScheduleForDate(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I navigate to next week")]
        public void WhenINavigateToNextWeek()
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should see mobile week schedule for '(.*)'")]
        public void ThenIShouldSeeMobileWeekScheduleFor(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I navigate to previous week")]
        public void WhenINavigateToPreviousWeek()
        {
            ScenarioContext.Current.Pending();
        }

        [When(@"I select week '(.*)' in the calendar")]
        public void WhenISelectWeekInTheCalendar(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"I should end up in month view for '(.*)'")]
        public void ThenIShouldEndUpInMonthViewFor(string p0)
        {
            ScenarioContext.Current.Pending();
        }

        private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
        {
            var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");
            var lastDayOfWeek = DateHelper.GetLastDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture).ToString("yyyy-MM-dd");

            Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}']", firstDayOfWeek));
            Browser.Interactions.AssertExists(string.Format(".weekview-day[data-mytime-date='{0}']", lastDayOfWeek));
        }
    }
}
