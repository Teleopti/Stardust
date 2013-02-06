using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestsPageStepDefinition
	{
		[When(@"I view Add Shift Trade Request")]
		public void WhenIViewAddShiftTradeRequest()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
			Pages.Pages.RequestsPage.AddRequestDropDown.EventualClick();
			Pages.Pages.RequestsPage.AddShiftTradeRequestMenuItem.EventualClick();
		}

		[When(@"I view Add Shift Trade Request for date '(.*)'")]
		public void WhenIViewAddShiftTradeRequestForDate(DateTime date)
		{
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
			Pages.Pages.RequestsPage.AddRequestDropDown.EventualClick();
			Pages.Pages.RequestsPage.AddShiftTradeRequestMenuItem.EventualClick();
			var dateAsSwedishString = date.ToShortDateString(CultureInfo.GetCultureInfo("sv-SE"));
			var script = string.Format("Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.SetShiftTradeRequestDate('{0}');", dateAsSwedishString);
			Browser.Current.Eval(script);
			EventualAssert.That(() => Pages.Pages.Current.Document.Span(Find.ById("Request-add-loaded-date")).Text, Is.EqualTo(dateAsSwedishString));
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
			EventualAssert.That(() => DateTime.Parse(Pages.Pages.RequestsPage.AddShiftTradeDatePicker.Text), Is.EqualTo(date));
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

	}
}
