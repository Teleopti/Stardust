using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class WeekSchedulePageStepDefinitions
	{
		[When(@"I click on the day symbol area for date '(.*)'")]
		public void WhenIClickOnTheDaySymbolAreaForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.Click(string.Format("ul.weekview-day[data-mytime-date='{0}'] li#day-symbol #add-request-cell", formattedDate));
		}

		[When(@"I click on the day summary for date '(.*)'")]
		public void WhenIClickOnTheDaySummaryForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.Click(string.Format("ul.weekview-day[data-mytime-date='{0}'] li#day-summary", formattedDate));
		}
	}
}