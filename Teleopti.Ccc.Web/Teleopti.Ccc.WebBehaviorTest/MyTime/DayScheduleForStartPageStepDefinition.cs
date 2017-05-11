using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
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

		[Then(@"I should see I have '(.*)' unread message\(s\)")]
		public void ThenIShouldSeeIHaveUnreadMessageS(string messageCount)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $(\"#MobileDayView-message\").length", messageCount);
		}

		[When(@"I click the message icon")]
		public void WhenIClickTheMessageIcon()
		{
			Browser.Interactions.Click(".mobile-message");
		}

		[Then(@"I could see the message with title '(.*)'")]
		public void ThenICouldSeeTheMessageWithTitle(string messageTitle)
		{
			Browser.Interactions.AssertAnyContains(".request-data-subject", messageTitle);
		}

		[Then(@"I should see the request icon")]
		public void ThenIShouldSeeTheRequestIcon()
		{
			BrowserInteractionsJQueryExtensions.AssertVisibleUsingJQuery(Browser.Interactions, "#MobileDayView-request");
		}

		[When(@"I click the request icon")]
		public void WhenIClickTheRequestIcon()
		{
			Browser.Interactions.Click(".mobile-request");
		}

		[Then(@"I should see it go to request page")]
		public void ThenIShouldSeeItGoToRequestPage()
		{
			Browser.Interactions.AssertUrlContains("MyTime#Requests/");
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
