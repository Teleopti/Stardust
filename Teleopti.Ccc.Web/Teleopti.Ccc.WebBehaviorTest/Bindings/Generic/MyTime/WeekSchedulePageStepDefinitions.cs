using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class WeekSchedulePageStepDefinitions
	{
		[When(@"I click on the day symbol area for date '(.*)'")]
		public void WhenIClickOnTheDaySymbolAreaForDate(DateTime date)
		{
			Browser.Interactions.Click(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-symbol", date.ToString("yyyy-MM-dd")));
		}

		[When(@"I click on the day summary for date '(.*)'")]
		public void WhenIClickOnTheDaySummaryForDate(DateTime date)
		{
			Browser.Interactions.Click(string.Format(".weekview-day[data-mytime-date='{0}'] .weekview-day-summary", date.ToString("yyyy-MM-dd")));
		}

		[When(@"I click overtime availability bar")]
		public void WhenIClickOvertimeAvailabilityBar()
		{
			Browser.Interactions.Click(".overtime-availability-bar");
		}

	}
}