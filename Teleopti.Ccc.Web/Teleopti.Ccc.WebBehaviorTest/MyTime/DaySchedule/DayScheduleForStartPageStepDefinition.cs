using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime.DaySchedule
{
	[Binding]
	class DayScheduleForStartPageStepDefinition
	{
		[Then(@"I should see mobile day view")]
		public void ThenIShouldSeeMobileDayView()
		{
			Browser.Interactions.AssertExists(".mobile-start-day");
		}

		[When(@"I open MyTime menu on the left")]
		[Then(@"I open MyTime menu on the left")]
		public void ThenIOpenMyTimeMenuOnTheLeft()
		{
			Browser.Interactions.Click(".navbar-toggle-button");
		}

		[When(@"I click Schedule menu item")]
		public void WhenIClickScheduleMenuItem()
		{
			Browser.Interactions.ClickContaining("ul li a", "Schedule");
		}

		[When(@"I am viewing mobile view for date '(.*)'")]
		public void WhenIAmViewingMobileViewForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileDaySchedulePage(date);
		}

		[When(@"I am viewing mobile view for tomorrow")]
		public void WhenIAmViewingMobileViewForTomorrow()
		{
			TestControllerMethods.Logon();
			Navigation.GotoMobileDaySchedulePage(DateTime.Now.AddDays(1));
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

		[Then(@"I should see shift trade request page on date '(.*)'")]
		public void ThenIShouldSeeShiftTradeRequestPageOnDate(string date)
		{
			Browser.Interactions.AssertUrlContains("Requests/Index/ShiftTrade/" + date);
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
			Browser.Interactions.Click("div.mobile-start-day button.fab");
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

		[When(@"I click menu Text Request")]
		public void WhenIClickMenuTextRequest()
		{
			Browser.Interactions.ClickContaining("li>a", "Text Request");
		}

		[When(@"I click menu Overtime Request")]
		public void WhenIClickMenuOvertimeRequest()
		{
			Browser.Interactions.ClickContaining("li>a", "Overtime Request");
		}

		[When(@"I click menu Shift Trade Request")]
		public void WhenIClickMenuShiftTradeRequest()
		{
			Browser.Interactions.ClickContaining("li>a", "Shift Trade Request");
		}

		[When(@"I click menu Post Shift for Trade")]
		public void WhenIClickMenuPostShiftForTrade()
		{
			Browser.Interactions.ClickContaining("li>a", "Post Shift for Trade");
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

		[When(@"I input '(.*)' as subject")]
		public void WhenIInputAsSubject(string subject)
		{
			Browser.Interactions.FillWith("input.request-new-subject", subject);
		}


	 

		[When(@"I click save Absence Report")]
		public void WhenIClickSaveAbsenceReport()
		{
			Browser.Interactions.Click("div.mobile-start-day button.absence-report-send");
			Browser.Interactions.Javascript_IsFlaky("window.setTimeout(function(){ window.location.reload();}, 1000);");
		}

		[When(@"I click save request")]
		public void WhenIClickSaveRequest()
		{
			Browser.Interactions.Click("div.mobile-start-day button.request-new-send");
		}


		[Then(@"I should see '(.*)' '(.*)' in schedule")]
		public void ThenIShouldSeeInSchedule(string timeSpan, string absenceType)
		{
			Browser.Interactions.AssertAnyContains("div>strong", absenceType);
			Browser.Interactions.AssertAnyContains("span.displayblock", timeSpan);
		}

		[Then(@"I should see '(.*)' Overtime Availability in schedule")]
		public void ThenIShouldSeeOvertimeAvailabilityInSchedule(string timeSpan)
		{
			Browser.Interactions.Click("div>i.glyphicon-time");
			Browser.Interactions.AssertAnyContains("div.tooltip-inner", timeSpan);
		}


		[When(@"I click show probability toggle")]
		public void WhenIClickShowProbabilityToggle()
		{
			Browser.Interactions.Click("div.probability-toggle-container>a");
		}

		[When(@"I click show absence probability")]
		public void WhenIClickShowAbsenceProbability()
		{
			Browser.Interactions.Click("input[name='probabilityRadios'][value='1']");
		}

		[When(@"I click show overtime probability")]
		public void WhenIClickShowOvertimeProbability()
		{
			Browser.Interactions.Click("input[name='probabilityRadios'][value='2']");
		}

		[When(@"I click hide probability")]
		public void WhenIClickHideProbability()
		{
			Browser.Interactions.Click("input[name='probabilityRadios'][value='0']");
		}

		[Then(@"I should see the probability in schedule")]
		public void ThenIShouldSeeTheProbabilityInSchedule()
		{
			Browser.Interactions.AssertExists("div.probability-cell");
		}

		[Then(@"I should not see the probability in schedule")]
		public void ThenIShouldNotSeeTheProbabilityInSchedule()
		{
			Browser.Interactions.AssertNotExists("div.mobile-schedule-container", "div.probability-cell");
		}

		[When(@"I change date to tomorrow")]
		public void WhenIChangeDateToTomorrow()
		{
			Browser.Interactions.Click("button>i.glyphicon-chevron-right");
		}

		[Then(@"I should see the selected probability toggle is Overtime Probability")]
		public void ThenIShouldSeeTheSelectedProbabilityToggleIsOvertimeProbability()
		{
			Browser.Interactions.AssertExists("span.overtime-probability-label");
		}

		[Given(@"tomorrow (I) have a full day absence")]
		public void GivenIHaveATomorrowShiftWith(string person)
		{
			DataMaker.Person(person).Apply(new FullDayAbsenceConfigurable
			{
				Scenario = DefaultScenario.Scenario.Description.Name,
				Name = "Vacation",
				Date = DateTime.Today.AddDays(1)
			});
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
