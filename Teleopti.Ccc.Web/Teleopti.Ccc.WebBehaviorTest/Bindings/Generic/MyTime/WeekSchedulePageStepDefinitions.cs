using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class WeekSchedulePageStepDefinitions
	{
		[When(@"I click on the day symbol area for date '(.*)'")]
		public void WhenIClickOnTheDaySymbolAreaForDate(DateTime date)
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(date).ListItems.First(Find.ById("day-symbol")).Div(Find.ById("add-request-cell")).EventualClick();
		}

		[When(@"I click on the day summary for date '(.*)'")]
		public void WhenIClickOnTheDaySummaryForDate(DateTime date)
		{
			Pages.Pages.WeekSchedulePage.DayElementForDate(date).ListItems.First(Find.ById("day-summary")).EventualClick();
		}
	}
}