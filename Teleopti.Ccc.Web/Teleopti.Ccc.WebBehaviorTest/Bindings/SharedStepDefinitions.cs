using System;
using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Pages.Common;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class SharedStepDefinitions
	{

		[When(@"I click next virtual schedule period button")]
		[When(@"I click the next day button")]
		public void WhenIClickNextVirtualSchedulePeriodButton()
		{
			Browser.Interactions.ClickUsingJQuery(".input-group button.done:has(i.glyphicon-chevron-right):visible");
		}

		[When(@"I click previous virtual schedule period button")]
		[When(@"I click the previous day button")]
		public void WhenIClickPreviousVirtualSchedulePeriodButton()
		{
            Browser.Interactions.ClickUsingJQuery(".input-group-btn button.done:has(i.glyphicon-chevron-left)");
		}

		[Then(@"I should see an error message")]
		public void ThenIShouldSeeAnErrorMessage()
		{
			Browser.Interactions.AssertExists("#error-panel");
		}

		[Then(@"I should see a symbol at the top of the schedule for date '(.*)'")]
		public void ThenIShouldSeeASymbolAtTheTopOfTheScheduleForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .glyphicon-comment", formattedDate));
		}

		[Then(@"I should see an overtime availability symbol with tooltip")]
		public void ThenIShouldSeeAnOvertimeAvailabilitySymbolWithTooltip(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityTooltipAndBar>();
			var formattedDate = overtimeAvailability.Date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .overtime-availability-symbol", formattedDate));
			Browser.Interactions.AssertKnockoutContextContains(string.Format(".weekview-day[data-mytime-date={0}]", formattedDate), "textOvertimeAvailabilityText()", overtimeAvailability.StartTime);
			Browser.Interactions.AssertKnockoutContextContains(string.Format(".weekview-day[data-mytime-date={0}]", formattedDate), "textOvertimeAvailabilityText()", overtimeAvailability.EndTime);
		}

		[Then(@"I should not see a symbol at the top of the schedule for date '(.*)'")]
		public void ThenIShouldNotSeeASymbolAtTheTopOfTheScheduleForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .glyphicon-comment", formattedDate));
		}

		[Then(@"I should not see an overtime availability symbol for date '(.*)'")]
		public void ThenIShouldNotSeeAnOvertimeAvailabilitySymbolForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .overtime-availability-symbol", formattedDate));
		}

		[Then(@"I should see a user-friendly message explaining I dont have anything to view")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingIDontHaveAnythingToView()
		{
			Browser.Interactions.AssertExists(".alert.alert-info");
		}

		[Then(@"I should see the virtual schedule period from '(.*)' to '(.*)'")]
		public void ThenIShouldSeeTheVirtualSchedulePeriod(DateTime start, DateTime end)
		{
			var appendSelectableClass = new Func<string, string>(s => s + ".ui-selectee");

			Browser.Interactions.AssertNotExistsUsingJQuery(appendSelectableClass(CalendarCells.DateSelector(start)), appendSelectableClass(CalendarCells.DateSelector(start.AddDays(-1))));
			Browser.Interactions.AssertNotExistsUsingJQuery(appendSelectableClass(CalendarCells.DateSelector(end)), appendSelectableClass(CalendarCells.DateSelector(end.AddDays(1))));
		}

		[Then(@"I should see a message saying I am missing a workflow control set")]
		public void ThenIShouldSeeAMessageSayingIAmMissingAWorkflowControlSet()
		{
			Browser.Interactions.AssertAnyContains(".alert", Resources.MissingWorkflowControlSet);
		}
	}
}
