using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using WatiN.Core;

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