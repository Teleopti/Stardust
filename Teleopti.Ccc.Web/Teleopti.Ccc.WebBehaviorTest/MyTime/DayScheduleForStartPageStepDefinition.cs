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

		[When(@"I am viewing mobile view for today")]
		public void WhenIAmViewingMobileViewForToday()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileDaySchedulePage(DateTime.Now.Date);
		}

		[Then(@"I should see my day view schedule with")]
		public void ThenIShouldSeeMyDayViewScheduleWith(Table table)
		{
			var content = table.CreateInstance<MobileDayScheduleContentItem>();

			Browser.Interactions.AssertAnyContains(".mobile-summary-content", content.ShiftCategory);
			Browser.Interactions.AssertAnyContains(".mobile-summary-content", content.TimeSpan);
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

		[When(@"I click the menu button in start page")]
		public void WhenIClickTheMenuButton()
		{
			Browser.Interactions.Click("div.mobile-start-day div.fab");
		}

		[When(@"I click menu Overtime Availability")]
		public void WhenIClickMenuOvertimeAvailability()
		{
			Browser.Interactions.ClickContaining("li>a", "Overtime Availability");
		}

		[When(@"I click menu menu Absence Reporting")]
		public void WhenIClickMenuMenuAbsenceReport()
		{
			Browser.Interactions.ClickContaining("li>a", "Absence Reporting");
		}

		[When(@"I click menu Absence Request")]
		public void WhenIClickMenuAbsenceRequest()
		{
			Browser.Interactions.ClickContaining("li>a", "Absence Request");

		}


		[When(@"I input '(.*)' as overtime startTime and '(.*)' as overtime endTime")]
		public void WhenIInputAsOvertimeStartTimeAndAsOvertimeEndTime(string startTime, string endTime)
		{
			Browser.Interactions.FillWith("input.overtime-availability-start-time", startTime);
			Browser.Interactions.FillWith("input.overtime-availability-end-time", endTime);
		}

		[When(@"I input '(.*)' as subject and '(.*)' as message")]
		public void WhenIInputAsSubjectAndAsMessage(string subject, string message)
		{
			Browser.Interactions.FillWith("input.request-new-subject", subject);
			Browser.Interactions.FillWith("textarea.request-new-message", message);
		}

		[When(@"I click save Overtime Availability")]
		public void WhenIClickSaveOvertimeAvailability()
		{
			Browser.Interactions.Click("div.mobile-start-day button.request-new-send");
		}

		[When(@"I click save Absence Report")]
		public void WhenIClickSaveAbsenceReport()
		{
			Browser.Interactions.Click("div.mobile-start-day button.absence-report-send");
			Browser.Interactions.Javascript("window.setTimeout(function(){ window.location.reload();}, 1000);");
		}

		[When(@"I click save Absence Request")]
		public void WhenIClickSaveAbsenceRequest()
		{
			Browser.Interactions.Click("div.mobile-start-day button.request-new-send");
		}


		[Then(@"I should see '(.*)' '(.*)' in schedule")]
		public void ThenIShouldSeeInSchedule(string timeSpan, string absenceType)
		{
			Browser.Interactions.AssertAnyContains("div>strong", absenceType);
			Browser.Interactions.AssertAnyContains("span.displayblock", timeSpan);
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
