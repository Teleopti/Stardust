using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Interfaces.Domain;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
    [Binding]
    public class WeekScheduleOnMobileStepDefinitions
    {
        [Then(@"I should see my mobile week schedule for date '(.*)'")]
        public void ThenIShouldSeeMyMobileWeekScheduleForDate(DateTime date)
        {
            AssertShowingWeekForDay(date.Date);
        }

        [Then(@"I should see the shift with")]
		public void ThenIShouldSeeTheShiftWith(Table table)
		{
			var shift = table.CreateInstance<ShiftListItem>();
			Browser.Interactions.AssertAnyContains(string.Format("[data-mytime-date='{0}'] .shift", shift.Date), shift.ShiftCategory);
			Browser.Interactions.AssertAnyContains(string.Format("[data-mytime-date='{0}'] .shift", shift.Date), shift.TimeSpan);
		}

		public class ShiftListItem
		{
			public string Date { get; set; }
			public string ShiftCategory { get; set; }
			public string TimeSpan { get; set; }
		}

        [Then(@"I should see the day off on '(.*)'")]
        public void ThenIShouldSeeTheDayOffOn(string date)
        {
			Browser.Interactions.AssertExists(string.Format("[data-mytime-date='{0}'] .dayoff", date));
        }

        private void AssertShowingWeekForDay(DateTime anyDayOfWeek)
        {
            var firstDayOfWeek = DateHelper.GetFirstDateInWeek(anyDayOfWeek, DataMaker.Data().MyCulture);

            foreach (var date in firstDayOfWeek.DateRange(7))
            {
                Browser.Interactions.AssertExistsUsingJQuery(string.Format(".day:contains('{0}')", date.ToString("yyyy-MM-dd")));
            }
        }

		  [Then(@"I should see the absence with")]
		  public void ThenIShouldSeeTheAbsenceWith(Table table)
		  {
			  var absence = table.CreateInstance<AbsenceListItem>();
			  Browser.Interactions.AssertAnyContains(string.Format("[data-mytime-date='{0}'] .absence", absence.Date), absence.Name);
		  }

		  public class AbsenceListItem
		  {
			  public string Name { get; set; }
			  public string Date { get; set; }
		  }

		  [Then(@"I should not see any shift for day '(.*)'")]
		  [Then(@"I should not see a shift on date '(.*)'")]
		  public void ThenIShouldNotSeeAnyShiftForDay(DateTime date)
		  {
			  Browser.Interactions.AssertNotExists(".weekview-mobile",
													string.Format("'[data-mytime-date='{0}']' .shift", date.ToString("yyyy-MM-dd")));
		  }

		  [Then(@"I should not see dayoff on date '(.*)'")]
		  public void ThenIShouldNotSeeDayoffOnDate(string date)
		  {
			  Browser.Interactions.AssertNotExists(".weekview-mobile", string.Format("[data-mytime-date='{0}'] .dayoff", date));
		  }

		  [Then(@"I should see a day name being '(.*)'")]
		  public void ThenIShouldSeeADayNameBeing(string name)
		  {
			  Browser.Interactions.AssertAnyContains(".weekday", name);
		  }

		  [Then(@"I should see '(.*)' as the first day")]
		  public void ThenIShouldSeeAsTheFirstDay(string date)
		  {
			  Browser.Interactions.AssertFirstContains(".date", date);
		  }

		  [Then(@"I should see '(.*)' as the first day of week label")]
		  public void ThenIShouldSeeAsTheFirstDayOfWeekLabel(string weekDayName)
		  {
			  Browser.Interactions.AssertFirstContains(".weekday", weekDayName);
		  }

		  [When(@"I navigate to next week")]
		  public void WhenINavigateToNextWeek()
		  {
			  Browser.Interactions.Click(".glyphicon-chevron-right");
		  }

		  [When(@"I navigate to previous week")]
		  public void WhenINavigateToPreviousWeek()
		  {
			  Browser.Interactions.Click(".glyphicon-chevron-left");
		  }


		  [When(@"I click the desktop link")]
		  public void WhenIClickTheDesktopLink()
		  {
			  Browser.Interactions.Click("#desktop");
		  }

		  [When(@"I click the mobile link")]
		  public void WhenIClickTheMobileLink()
		  {
			  Browser.Interactions.Click("#mobile");
		  }

    }
}
