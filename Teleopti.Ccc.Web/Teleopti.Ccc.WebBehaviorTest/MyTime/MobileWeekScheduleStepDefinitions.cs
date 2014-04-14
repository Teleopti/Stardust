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
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class MobileWeekScheduleStepDefinitions
    {
       

        [Then(@"I should see mobile view of the week with for date '(.*)'")]
        public void ThenIShouldSeeMobileViewOfTheWeekWithForDate(DateTime date)
        {
            AssertShowingWeekForDay(DateHelper.GetFirstDateInWeek(date.Date, DataMaker.Data().MyCulture));
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
