using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

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
			Navigation.GotoRequests();
			Browser.Interactions.Click(".shifttrade-request-add");
			Browser.Interactions.AssertExists(".ready-loading-flag.is-ready-loaded");
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
			var leavingDateForUser = new LeavingDateForUser { LeavingDate = leavingDate };
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

		
		[Then(@"Add Shift Trade Request view should not be visible")]
		public void ThenAddShiftTradeRequestViewShouldNotBeVisible()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-add-shift-trade");
		}

		[Then(@"I should see details with message that tells the user that the status of the shifttrade is new")]
		public void ThenIShouldSeeDetailsWithMessageThatTellsTheUserThatTheStatusOfTheShifttradeIsNew()
		{
			Browser.Interactions.AssertVisibleUsingJQuery("#Request-shift-trade-detail-info");
		}

		[Then(@"I should not see timelines")]
		public void ThenIShouldNotSeeTimelines()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".shift-trade-swap-detail-timeline");
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

		[Given(@"I have possible shift trades with")]
		public void GivenIHavePossibleShiftTradesWith(Table table)
		{
			var scheduledAgentsForShiftTrade = table.CreateInstance<ScheduledAgentsForShiftTrade>();
			DataMaker.Data().Apply(scheduledAgentsForShiftTrade);
		}

		[Given(@"I can see '(.*)' possible shift trades")]
		[Then(@"I can see '(.*)' possible shift trades")]
		public void GivenICanSeePossibleShiftTrades(int possibleShiftTradeCount)
		{
			const string script = "return $('.shift-trade-person-schedule-row').length";
			Browser.Interactions.AssertJavascriptResultContains(script, possibleShiftTradeCount.ToString(CultureInfo.InvariantCulture));
		}

		[When(@"I scroll down to the bottom of the shift trade section")]
		public void WhenIScrollDownToTheBottomOfTheShiftTradeSection()
		{
			Browser.Interactions.Javascript("$(document).scrollTop($(document).height());");
			Browser.Interactions.AssertExists(".ready-loading-flag.is-ready-loaded");
		}

		[Then(@"the option '(.*)' should be selected")]
		public void ThenTheOptionShouldBeSelected(string optionSelected)
		{
			Select2Box.AssertSelectedOptionText("Team-Picker", optionSelected);
		}

		[When(@"I select the '(.*)'")]
		public void WhenISelectThe(string optionToSelect)
		{
			Select2Box.OpenWhenOptionsAreLoaded("Team-Picker");
			Select2Box.SelectItemByText("Team-Picker", optionToSelect);
		}
	}
}
