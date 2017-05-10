using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	class DayScheduleForStartPageStepDefinition
	{
		[When(@"I am viewing mobile view for date '(.*)'")]
		public void WhenIAmViewingMobileViewForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileDaySchedulePage(date);
		}

		[Then(@"I should see my day view schedule with")]
		public void ThenIShouldSeeMyDayViewScheduleWith(Table table)
		{
			var content = table.CreateInstance<MobileDayScheduleContentItem>();
		    var date = "return $(\".date-input-style\").val()";
		    
			Browser.Interactions.AssertAnyContains(".mobile-summary-content", content.WeekDay);
			Browser.Interactions.AssertAnyContains(".mobile-summary-content", content.ShiftCategory);
			Browser.Interactions.AssertAnyContains(".mobile-summary-content", content.TimeSpan);
			Browser.Interactions.AssertJavascriptResultContains(date, content.Date);
		}

		[When(@"I navigate to next day")]
		public void WhenINavigateToNextDay()
		{
			Browser.Interactions.Click(".glyphicon-chevron-right");
		}

		[When(@"I navigate to previous day")]
		public void WhenINavigateToPreviousDay()
		{
			Browser.Interactions.Click(".glyphicon-chevron-left");
		}

		[When(@"I click today button")]
		public void WhenIClickTodayOnTheTopOfTheView()
		{
			Browser.Interactions.Click(".glyphicon-home");
		}

		public class MobileDayScheduleContentItem
	    {
	        public string Date { get; set; }
			public string ShiftCategory { get; set; }
			public string TimeSpan { get; set; }
			public string WeekDay { get; set; }
		}

	}
}
