using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class MobileMonthStepDefinitions
	{
		[When(@"when I see mobile day view")]
		public void WhenWhenISeeMobileDayView()
		{
			Browser.Interactions.AssertExists(".mobile-start-day");
		}

		[When(@"I click month view icon")]
		public void WhenIClickMonthViewIcon()
		{
			Browser.Interactions.ClickUsingJQuery(".mobile-month-calendar-icon i.glyphicon-calendar");
		}

		[Then(@"I should see mobile month view")]
		public void ThenIShouldSeeMobileMonthView()
		{
			Browser.Interactions.AssertExistsUsingJQuery(".mobile-month-view");
			Browser.Interactions.AssertJavascriptResultContains("return $('.mobile-month-view .mobile-month-row-fluid').length", "6");
		}

		[Then(@"I should see schedule summary with start time '(.*)' and end time '(.*)' on '(.*)' on mobile month")]
		public void ThenIShouldSeeScheduleSummaryWithStartTimeAndEndTimeOnOnMobileMonth(string start, string end, string date)
		{
			Browser.Interactions.AssertExistsUsingJQuery($".mobile-month-view .mobile-month-cell[date='{date}']");
			Browser.Interactions.AssertFirstContainsUsingJQuery($".mobile-month-view .mobile-month-cell[date='{date}']", start);
			Browser.Interactions.AssertFirstContainsUsingJQuery($".mobile-month-view .mobile-month-cell[date='{date}']", end);
		}
	}
}
