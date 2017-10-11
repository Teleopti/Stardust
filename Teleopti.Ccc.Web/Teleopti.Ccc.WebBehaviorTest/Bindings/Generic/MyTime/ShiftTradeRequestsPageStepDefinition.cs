﻿using System;
using System.Globalization;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.MyTime
{
	[Binding]
	public class ShiftTradeRequestsPageStepDefinition
	{
		[When(@"I view Add Shift Trade Request")]
		public void WhenIViewAddShiftTradeRequest()
		{
			gotoAddRequestToday();
		}

		[Given(@"I view Add Shift Trade Request for date '(.*)'")]
		[When(@"I view Add Shift Trade Request for date '(.*)'")]
		public void WhenIViewAddShiftTradeRequestForDate(DateTime date)
		{
			gotoAddRequestToday();
			var dateAsSwedishString = date.ToShortDateString(CultureInfo.GetCultureInfo("sv-SE"));
			var script = string.Format("return Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.SetShiftTradeRequestDate('{0}');", dateAsSwedishString);
			Browser.Interactions.AssertJavascriptResultContains(script, dateAsSwedishString);
			Browser.Interactions.AssertExists(".ready-loading-flag.is-ready-loaded");
		}

		private static void gotoAddRequestToday()
		{
			TestControllerMethods.Logon();

			Browser.Interactions.TryUntil(
				() =>
				{
					Navigation.GotoRequests();
					Browser.Interactions.Click("#addShiftTradeRequest");
				},
				() => Browser.Interactions.IsExists(".ready-loading-flag.is-ready-loaded"),
				TimeSpan.FromMilliseconds(2000));
		}

		private static void gotoShiftTradeBulletinBoardToday()
		{
			TestControllerMethods.Logon();

			Browser.Interactions.TryUntil(
				() =>
				{
					Navigation.GotoRequests();
					Browser.Interactions.Click("#addShiftTradeRequestFromBulletinBoard");
				},
				() => Browser.Interactions.IsExists(".bulletin-ready-loading-flag.is-ready-loaded"),
				TimeSpan.FromMilliseconds(1000));
		}

		private static bool theGauntlet(string owner)
		{
			if (owner == "anonym")
				return Browser.Interactions.IsContain("#Request-shift-trade-bulletin-board .shift-trade-agent-name", "Anonym");
			if (owner == "my")
				return Browser.Interactions.IsVisible(".shift-trade-my-schedule .shift-trade-layer");

			return Browser.Interactions.IsVisible("#agent-in-bulletin-board");

		}

		[When(@"I see '(.*)' shift on Shift Trade Bulletin Board on date '(.*)'")]
		public void WhenISeeWhoseShiftOnDate(string owner, DateTime date)
		{
			var dateAsSwedishString = date.ToShortDateString(CultureInfo.GetCultureInfo("sv-SE"));
			var script =
				$"Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.IsRunningBehaviorTest(); return Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.SetShiftTradeBulletinBoardDate('{dateAsSwedishString}');";

			gotoShiftTradeBulletinBoardToday();

			Browser.Interactions.TryUntil(
				() => Browser.Interactions.AssertJavascriptResultContains(script, dateAsSwedishString),
				() => theGauntlet(owner),
				TimeSpan.FromMilliseconds(1000));
		}

		[When(@"I see anonym shift on Shift Trade Bulletin Board on date '(.*)'")]
		public void WhenISeeAnonymShiftOnDate(DateTime date)
		{
			var dateAsSwedishString = date.ToShortDateString(CultureInfo.GetCultureInfo("sv-SE"));
			var script =
				$"Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.IsRunningBehaviorTest(); return Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.SetShiftTradeBulletinBoardDate('{dateAsSwedishString}');";

			gotoShiftTradeBulletinBoardToday();

			Browser.Interactions.TryUntil(
				() => Browser.Interactions.AssertJavascriptResultContains(script, dateAsSwedishString),
				() => Browser.Interactions.IsContain("#Request-shift-trade-bulletin-board .shift-trade-agent-name", "Anonym"),
				TimeSpan.FromMilliseconds(1000));
		}

		[Then(@"I should see a message text saying I am missing a workflow control set")]
		public void ThenIShouldSeeAMessageTextSayingIAmMissingAWorkflowControlSet()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".shift-trade-missing-wcs-message");
		}

		[Given(@"I should see a message text saying that no possible shift trades could be found")]
		[Then(@"I should see a message text saying that no possible shift trades could be found")]
		public void ThenIShouldSeeAMessageTextSayingThatNoPossibleShiftTradesCouldBeFound()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".shift-trade-no-possible-trades");
		}

		[Then(@"I should see a message text saying that I have no access to any teams")]
		public void ThenIShouldSeeAMessageTextSayingThatIHaveNoAccessToAnyTeams()
		{
			Browser.Interactions.AssertVisibleUsingJQuery(".shift-trade-missing-team-message");
		}

		[Given(@"my last working date as an agent in the organisation is '(.*)'")]
		public void GivenMyLastWorkingDateAsAnAgentInTheOrganisationIs(DateTime leavingDate)
		{
			var leavingDateForUser = new PersonUserConfigurable {TerminalDate = leavingDate};
			DataMaker.Data().Apply(leavingDateForUser);
		}

		[Then(@"I should see my schedule with")]
		public void ThenIShouldSeeMyScheduleWith(Table table)
		{
			var expectedTimes = table.Rows[0][1] + " - " + table.Rows[1][1];

			Browser.Interactions.AssertExists(".shift-trade-my-schedule .shift-trade-layer");
			Browser.Interactions.AssertExists(string.Format(".shift-trade-my-schedule .shift-trade-layer[scheduleinfo*='{0}']", expectedTimes));
		}

		[Then(@"I should only see a possible schedule trade with '(.*)'")]
		public void ThenIShouldOnlySeeAPossibleScheduleTradeWith(string name)
		{
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".shift-trade-agent-name:visible:contains('{0}')", name));
			Browser.Interactions.AssertJavascriptResultContains("return $('.agent:visible').length;", "1");
		}

		[Then(@"I should see a possible schedule trade with '(.*)'")]
		public void ThenIShouldSeeAPossibleScheduleTradeWith(string name)
		{
			Browser.Interactions.AssertAnyContains(".shift-trade-agent-name", name);
		}

		[Then(@"I should not see agent name in the possible schedule trade list")]
		public void ThenIShouldNotSeeThePossibleScheduleTradeWith()
		{
			Browser.Interactions.AssertAnyContains(".shift-trade-agent-name", "");
		}

		[Then(@"I should see agent name as Anonym")]
		public void ThenIShouldSeeAgentNameAsAnonym()
		{
			Browser.Interactions.AssertExists(".shift-trade-agent-name");
			Browser.Interactions.AssertAnyContains("#Request-shift-trade-bulletin-board .shift-trade-agent-name", "Anonym");
		}

		[Then(@"I should see '(.*)' first in the list")]
		public void ThenIShouldSeeFirstInTheList(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-person-schedule-row:first-child:contains('{0}')", agentName));
		}

		[Then(@"I should see '(.*)' last in the list")]
		public void ThenIShouldSeeLastInTheList(string agentName)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".shift-trade-person-schedule-row:first-child:contains('{0}')", agentName));
		}

		[Then(@"I should not see a possible schedule trade with '(.*)'")]
		public void ThenIShouldNotSeeAPossibleScheduleTradeWith(string name)
		{
			Browser.Interactions.AssertNotExistsUsingJQuery(".shift-trade-my-schedule-row", string.Format(".shift-trade-agent-name:contains('{0}')", name));
		}

		[Then(@"the selected date should be '(.*)'")]
		public void ThenTheSelectedDateShouldBe(DateTime date)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.add-shifttrade-datepicker').val();", date.Year.ToString());
			Browser.Interactions.AssertJavascriptResultContains("return $('.add-shifttrade-datepicker').val();", date.Month.ToString());
			Browser.Interactions.AssertJavascriptResultContains("return $('.add-shifttrade-datepicker').val();", date.Day.ToString());
		}

		[Then(@"I cannot navigate to the previous date")]
		public void ThenICannotNavigateToThePreviousDate()
		{
			Browser.Interactions.AssertExists(".previous-date:disabled");
		}

		[Then(@"I cannot navigate to the bulletin previous date")]
		public void ThenICannotNavigateToTheBulletinPreviousDate()
		{
			Browser.Interactions.AssertExists(".bulletin-previous-date:disabled");
		}


		[Then(@"I cannot navigate to the next date")]
		public void ThenICannotNavigateToTheNextDate()
		{
			Browser.Interactions.AssertExists(".next-date:disabled");
		}

		[When(@"I click on the next date")]
		public void WhenIClickOnTheNextDate()
		{
            Browser.Interactions.Click(".next-date");
		}

		[When(@"I click on the previous date")]
		public void WhenIClickOnThePreviousDate()
		{
			Browser.Interactions.Click(".previous-date");
		}

		[When(@"I select date '(.*)' by calender")]
		public void WhenISelectDateByCalender(DateTime date)
		{
			Browser.Interactions.Click("#Request-add-shift-trade .glyphicon-calendar");
			string selector = string.Format(".datepicker-days .day:contains('{0}')", date.Day);
			Browser.Interactions.ClickUsingJQuery(selector);
		}

		[Then(@"I should see the time line hours span from '(.*)' to '(.*)'")]
		public void ThenIShouldSeeTheTimeLineHoursSpanFromTo(string timeLineHourFrom, string timeLineHourTo)
		{
			Browser.Interactions.AssertJavascriptResultContains("return $('.shift-trade-label:lt(1)').text() + ' ' + $('.shift-trade-label:lt(2)').text();", timeLineHourFrom);
			Browser.Interactions.AssertFirstContains(".shift-trade-label:last-child", timeLineHourTo);
		}

		[Then(@"I should see details with a schedule from")]
		public void ThenIShouldSeeDetailsWithAScheduleFrom(Table table)
		{
			var expectedScheduleTimePeriod = table.Rows[0][1] + "-" + table.Rows[1][1];
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(
				".shift-trade-layer.edit.my:first()[scheduleinfo*='{0}']", expectedScheduleTimePeriod));
		}
		[Then(@"I should see details with a schedule to")]
		public void ThenIShouldSeeDetailsWithAScheduleTo(Table table)
		{
			var expectedScheduleTimePeriod = table.Rows[0][1] + "-" + table.Rows[1][1];
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(
				".shift-trade-layer.edit.other:first()[scheduleinfo*='{0}']", expectedScheduleTimePeriod));
		}


		[Then(@"I should see my details scheduled day off '(.*)'")]
		public void ThenIShouldSeeMyDetailsScheduledDayOff(string dayOffText)
		{
			Browser.Interactions.AssertFirstContains(".shift-trade-swap-detail-schedule .shift-trade-dayoff-name", dayOffText);
		}

		[Then(@"I should see other details scheduled day off '(.*)'")]
		public void ThenIShouldSeeOtherDetailsScheduledDayOff(string dayOffText)
		{
			Browser.Interactions.AssertFirstContains(".shift-trade-swap-detail-schedule-to .shift-trade-dayoff-name", dayOffText);
		}

		[Then(@"I should see details with subject '(.*)'")]
		public void ThenIShouldSeeDetailsWithSubject(string subject)
		{
			Browser.Interactions.AssertFirstContains(".request-data-subject", subject);
		}

		[Then(@"I should see details with message '(.*)'")]
		public void ThenIShouldSeeDetailsWithMessage(string message)
		{
			var fiftyFirstCharsOfMessage = message.Substring(0, 50);
			Browser.Interactions.AssertFirstContains(".request-text", fiftyFirstCharsOfMessage);
		}

		[Then(@"I should see trade date in detail as '(.*)'")]
		public void ThenIShouldSeeTradeDateInDetail(string date)
		{
			Browser.Interactions.AssertFirstContains(".trade-day-detail .trade-date", date);
		}

		[Then(@"I should see shift trade date as '(.*)'")]
		public void ThenIShouldSeeShiftTradeDate(string date)
		{
			Browser.Interactions.AssertFirstContains(".request-body .request-data-date", date);
		}

		[When(@"I enter subject '(.*)'")]
		public void WhenIEnterSubject(string subject)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-shift-trade-subject-input", subject);
		}

		[When(@"I enter message '(.*)'")]
		public void WhenIEnterMessage(string message)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Request-add-shift-trade-message-input", message);
		}

		[When(@"I click send shifttrade button")]
		public void WhenIClickSendShifttradeButton()
		{
			Browser.Interactions.Click(".send-button");
		}

		[When(@"I click send button in bulletin board")]
		[Given(@"I click send button in bulletin board")]
		public void WhenIClickSendButtonInBulletinBoard()
		{
			Browser.Interactions.Click("#Shift-trade-bulletin-board-send");
		}

		[Then(@"Add Shift Trade Request view should not be visible")]
		public void ThenAddShiftTradeRequestViewShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-add-shift-trade");
		}

		[Then(@"Shift trade bulletin board view should not be visible")]
		public void ThenShiftTradeBulletinBoardViewShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-shift-trade-bulletin-board");
		}

		[Then(@"I should see details with message that tells the user that the status of the shifttrade is new")]
		public void ThenIShouldSeeDetailsWithMessageThatTellsTheUserThatTheStatusOfTheShifttradeIsNew()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-shift-trade-detail-info");
		}

		[Then(@"I should not see timelines")]
		public void ThenIShouldNotSeeTimelines()
		{
			Browser.Interactions.AssertNotExists("#sender-info", ".shift-trade-swap-detail-timeline");
		}

		[When(@"I click cancel button")]
		public void WhenIClickCancelButton()
		{
			Browser.Interactions.Click(".cancel-button");
		}

		[Then(@"I should navigate to shift trade for '(.*)'")]
        public void ThenIShouldNavigateToShiftTradeFor(DateTime date)
        {
            Browser.Interactions.AssertUrlContains("ShiftTrade/"+date.ToString("yyyyMMdd"));
        }

		[Then(@"the option '(.*)' should be selected")]
		public void ThenTheOptionShouldBeSelected(string optionSelected)
		{
			Select2Box.AssertSelectedOptionText("Team-Picker", optionSelected);
		}

		[Then(@"the option for site filter '(.*)' should be selected")]
		public void ThenTheOptionForSiteFilterShouldBeSelected(string optionSelected)
		{
			Select2Box.AssertSelectedOptionText("Site-Picker", optionSelected);
		}

		[When(@"I select the '(.*)'")]
		public void WhenISelectThe(string optionToSelect)
		{
			Select2Box.OpenWhenOptionsAreLoaded("Team-Picker");
			Select2Box.SelectItemByText("Team-Picker", optionToSelect);
		}

		[When(@"I select the site filter '(.*)'")]
		public void WhenISelectTheSiteFilter(string optionToSelect)
		{
			Select2Box.OpenWhenOptionsAreLoaded("Site-Picker");
			Select2Box.SelectItemByText("Site-Picker", optionToSelect);
		}

		[When(@"I type '(.*)' in the name search box")]
		public void WhenITypeInTheNameSearchBox(string partialName)
		{
			var selector = ".name-search input.form-control";
			Browser.Interactions.FillWith(selector, partialName);
			Browser.Interactions.PressEnter(selector);
		}


		[Then(@"I should see MySchedule is dayoff")]
		public void ThenIShouldSeeMyScheduleIsDayoff()
		{
			Browser.Interactions.AssertExists(".shift-trade-my-schedule .shift-trade-layer-container .dayoff");
		}

		[Given(@"I choose '(.*)' to make a shift trade")]
		[When(@"I choose '(.*)' to make a shift trade")]
		[Then(@"I choose '(.*)' to make a shift trade")]
		public void WhenIChooseToMakeAShiftTrade(string p0)
		{
			Browser.Interactions.Click(".shift-trade-possible-trade-schedule .shift-trade-layer-container .shift-trade-layer");
		}

		[When(@"I add '(.*)' to my shift trade list")]
		[Given(@"I add '(.*)' to my shift trade list")]
		public void WhenIAddToMyShiftTradeList(string p0)
		{
			Browser.Interactions.ClickUsingJQuery("#shift-trade-add");
		}

		[Then(@"I should see '(.*)' in my shift trade list for date '(.*)'")]
		public void ThenIShouldSeeInMyShiftTradeListForDate(string agentName, string date)
		{
			Browser.Interactions.AssertAnyContains("#choose-history-list", agentName);
			Browser.Interactions.AssertAnyContains("#choose-history-list .trade-date", date);
		}

		[When(@"I remove the selected day from the shift trade list")]
		public void WhenIRemoveTheSelectedDayFromTheShiftTradeList()
		{
			Browser.Interactions.Click("#shift-trade-remove");
		}

		[Then(@"I should not see schedule on date '(.*)' in my shift trade list with '(.*)'")]
		public void ThenIShouldNotSeeScheduleDayForInMyShiftTradeListWith(string date, string agentName)
		{
			Browser.Interactions.AssertNoContains("#Request-all-permitted-teams", "#Request-all-permitted-teams .trade-date", agentName);
			Browser.Interactions.AssertNoContains("#Request-all-permitted-teams", "#Request-all-permitted-teams .shift-trade-agent-name", agentName + '$');
		}

		[Then(@"I should see '(.*)' can be added for date '(.*)'")]
		public void ThenIShouldSeeCanBeAddedForDate(string agentName, string date)
		{
			Browser.Interactions.AssertAnyContains(".shift-trade-person-schedule-row.agent", agentName);
			Browser.Interactions.AssertInputValueUsingJQuery(".add-shifttrade-datepicker", date);
			Browser.Interactions.AssertExists("#shift-trade-add");
		}

		[Then(@"I should see '(.*)' for date '(.*)' at top of my shift trade list")]
		public void ThenIShouldSeeForDateAtTopOfMyShiftTradeList(string agentName, string date)
		{
			Browser.Interactions.AssertFirstContains("#choose-history-list", agentName);
			Browser.Interactions.AssertFirstContains("#choose-history-list .trade-date", date);
		}

		[Then(@"I should see a confirm message on bulletin trade board")]
		public void ThenIShouldSeeAConfirmMessageOnBulletinTradeBoard()
		{
			Browser.Interactions.TryUntil(
				() => Browser.Interactions.Click("#agent-in-bulletin-board"),
				() => Browser.Interactions.IsVisible("#Request-add-shift-trade-detail-section"),
				TimeSpan.FromMilliseconds(1000));

			Browser.Interactions.AssertAnyContains("#Request-add-shift-trade-detail-section", Resources.SureToMakeShiftTrade);
		}

		[When(@"I click OtherAgent shift")]
		[Given(@"I click OtherAgent shift")]
		public void WhenIClickOtherAgentShift()
		{
			Browser.Interactions.TryUntil(
				() => Browser.Interactions.Click("#agent-in-bulletin-board"),
				() => Browser.Interactions.IsVisible("#Request-add-shift-trade-detail-section"),
				TimeSpan.FromMilliseconds(1000));
		}

		[Then(@"I should not see the agent name in detail view")]
		public void ThenIShouldNotSeeTheAgentNameInDetailView()
		{
			Browser.Interactions.AssertNotExists("#shift-trade-request-detail-view", "#sender-info");
			Browser.Interactions.AssertNotExists("#shift-trade-request-detail-view", "#reciever-info");
		}

	}
}
