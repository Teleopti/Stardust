﻿using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserInteractions;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
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

		[When(@"I click '(.*)'")]
		public void WhenIClick(string p0)
		{
			Browser.Interactions.Click(".cancel-button");
		}


		[When(@"I view Add Shift Trade Request for date '(.*)'")]
		public void WhenIViewAddShiftTradeRequestForDate(DateTime date)
		{
			gotoAddRequestToday();
			var dateAsSwedishString = date.ToShortDateString(CultureInfo.GetCultureInfo("sv-SE"));
			var script = string.Format("Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.SetShiftTradeRequestDate('{0}');", dateAsSwedishString);
			Browser.Interactions.Javascript(script);
			Browser.Interactions.AssertContains("#Request-add-loaded-date", dateAsSwedishString);
		}

		private static void gotoAddRequestToday()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
			Browser.Interactions.Click(".shifttrade-request-add");
			EventualAssert.That(() => string.IsNullOrEmpty(Pages.Pages.Current.Document.Span(Find.ById("Request-add-loaded-date")).Text), Is.False);
		}

		[Then(@"I should see a message text saying I am missing a workflow control set")]
		public void ThenIShouldSeeAMessageTextSayingIAmMissingAWorkflowControlSet()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeMissingWorkflowControlsSetMessage.DisplayVisible(), Is.True);
		}

		[Then(@"I should see a message text saying that no possible shift trades could be found")]
		public void ThenIShouldSeeAMessageTextSayingThatNoPossibleShiftTradesCouldBeFound()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeNoPossibleShiftTradesMessage.DisplayVisible(), Is.True);
		}

		[Then(@"I should see my schedule with")]
		public void ThenIShouldSeeMyScheduleWith(Table table)
		{
			var expectedTimes = table.Rows[0][1] + "-" + table.Rows[1][1];

			EventualAssert.That(() => Pages.Pages.RequestsPage.MyScheduleLayers.Any(), Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.MyScheduleLayers[0].Title, Contains.Substring(expectedTimes));
		}

		[Then(@"I should only see (.*)'s schedule")]
		public void ThenIShouldOnlySeeOtherAgentSSchedule(string agentName)
		{
			EventualAssert.That(() => Pages.Pages.Current.Document.Divs.Filter(QuicklyFind.ByClass("agent")).Count(div => div.IsDisplayed()), Is.EqualTo(1));
			EventualAssert.That(() => Pages.Pages.Current.Document.Divs.Filter(QuicklyFind.ByClass("agent")).First(div => div.IsDisplayed()).Text, Is.StringContaining(agentName));
		}

		[Then(@"I should see (.*) in the shift trade list")]
		public void ThenIShouldSeeOtherAgentSSchedule(string agentName)
		{
			EventualAssert.That(() => Pages.Pages.Current.Document.Divs.Filter(QuicklyFind.ByClass("agent")).Any(div => div.IsDisplayed() && div.Text.Trim() == agentName), Is.True);
		}


		[Then(@"I should see a possible schedule trade with")]
		public void ThenIShouldSeeAPossibleScheduleTradeWith(Table table)
		{
			var expectedTimes = table.Rows[0][1] + "-" + table.Rows[1][1];

			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeScheduleLayers.Any(), Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeScheduleLayers[0].Title, Contains.Substring(expectedTimes));
		}

		[Then(@"the selected date should be '(.*)'")]
		public void ThenTheSelectedDateShouldBe(DateTime date)
		{
			Browser.Interactions.AssertJavascriptResultContains("$('.add-shifttrade-datepicker').val();", date.Year.ToString());
			Browser.Interactions.AssertJavascriptResultContains("$('.add-shifttrade-datepicker').val();", date.Month.ToString());
			Browser.Interactions.AssertJavascriptResultContains("$('.add-shifttrade-datepicker').val();", date.Day.ToString());
		}

		[Then(@"I should see the time line hours span from '(.*)' to '(.*)'")]
		public void ThenIShouldSeeTheTimeLineHoursSpanFromTo(string timeLineHourFrom, string timeLineHourTo)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeTimeLineItems.Any(), Is.True);

			Span firstHour = Pages.Pages.RequestsPage.AddShiftTradeTimeLineItems.First().EventualGet();
			Span alternativeFirstHour = Pages.Pages.RequestsPage.AddShiftTradeTimeLineItems[1].EventualGet();
			if (string.IsNullOrEmpty(firstHour.Text))
				firstHour = alternativeFirstHour;

			Assert.That(firstHour.Text, Is.EqualTo(timeLineHourFrom));
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeTimeLineItems.Last().Text, Is.EqualTo(timeLineHourTo));
		}

		[Then(@"I should not see the datepicker")]
		public void ThenIShouldNotSeeTheDatepicker()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeDatePicker.Parent.DisplayVisible(), Is.False);
		}

		[Then(@"I should see my scheduled day off '(.*)'")]
		public void ThenIShouldSeeMyScheduledDayOff(string dayOffName)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.MyScheduleLayers.Count, Is.EqualTo(1));
			EventualAssert.That(() => Pages.Pages.RequestsPage.MyScheduleLayers.First().Span(Find.First()).Text, Is.EqualTo(dayOffName));
		}

		[Then(@"I should see details with a schedule from")]
		public void ThenIShouldSeeDetailsWithAScheduleFrom(Table table)
		{
			var expectedStart = table.Rows[0][1];
			var expectedEnd = table.Rows[1][1];

			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsFromScheduleLayers.Any(), Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsFromScheduleLayers.First().Title, Contains.Substring(expectedStart));
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsFromScheduleLayers.Last().Title, Contains.Substring(expectedEnd));
		}

		[Then(@"I should see my details scheduled day off '(.*)'")]
		public void ThenIShouldSeeMyDetailsScheduledDayOff(string dayOffText)
		{
			EventualAssert.That(()=>Pages.Pages.RequestsPage.MyScheduleDayOff.Text,Is.EqualTo(dayOffText));
		}

		[Then(@"I should see other details scheduled day off '(.*)'")]
		public void ThenIShouldSeeOtherDetailsScheduledDayOff(string dayOffText)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.OtherScheduleDayOff.Text, Is.EqualTo(dayOffText));
		}

		[Then(@"I should see details with a schedule to")]
		public void ThenIShouldSeeDetailsWithAScheduleTo(Table table)
		{
			
			var expectedStart = table.Rows[0][1];
			var expectedEnd = table.Rows[1][1];

			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsToScheduleLayers.Any(), Is.True);
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsToScheduleLayers.First().Title, Contains.Substring(expectedStart));
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeDetailsToScheduleLayers.Last().Title, Contains.Substring(expectedEnd));

		}

		[Then(@"I should see details with subject '(.*)'")]
		public void ThenIShouldSeeDetailsWithSubject(string subject)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeRequestDetailSubject.Text, Is.EqualTo(subject));
		}

		[Then(@"I should see details with message '(.*)'")]
		public void ThenIShouldSeeDetailsWithMessage(string message)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeRequestDetailMessage.Text, Is.EqualTo(message));
		}


		[When(@"I enter subject '(.*)'")]
		public void WhenIEnterSubject(string subject)
		{
			Pages.Pages.RequestsPage.AddShiftTradeSubject.WaitUntilDisplayed();
			Pages.Pages.RequestsPage.AddShiftTradeSubject.ChangeValue(subject);
		}

		[When(@"I enter message '(.*)'")]
		public void WhenIEnterMessage(string message)
		{
			Pages.Pages.RequestsPage.AddShiftTradeMessage.WaitUntilDisplayed();
			Pages.Pages.RequestsPage.AddShiftTradeMessage.ChangeValue(message);
		}
		
		[Then(@"Add Shift Trade Request view should not be visible")]
		public void ThenAddShiftTradeRequestViewShouldNotBeVisible()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeContainer.DisplayVisible(), Is.False);
		}

		[Then(@"I should see details with message that tells the user that the status of the shifttrade is new")]
		public void ThenIShouldSeeDetailsWithMessageThatTellsTheUserThatTheStatusOfTheShifttradeIsNew()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeRequestDetailInfo.Text, Is.EqualTo(Resources.CannotDisplayScheduleWhenShiftTradeStatusIsNew));
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShiftTradeRequestDetailInfo.IsDisplayed(), Is.True);
		}

		[Then(@"I should not see timelines")]
		public void ThenIShouldNotSeeTimelines()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.Timelines.Any(div=>div.IsDisplayed()), Is.False);
		}

	}
}
