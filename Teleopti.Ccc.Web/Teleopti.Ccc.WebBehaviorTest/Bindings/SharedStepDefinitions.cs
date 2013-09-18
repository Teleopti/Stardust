﻿using System;
using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Bindings.Generic;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core.Legacy;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;
using Teleopti.Ccc.WebBehaviorTest.Pages;
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
			Browser.Interactions.Click(".navbar-form button:nth-of-type(3)");
		}

		[When(@"I click previous virtual schedule period button")]
		[When(@"I click the previous day button")]
		public void WhenIClickPreviousVirtualSchedulePeriodButton()
		{
			Pages.Pages.CurrentDateRangeSelector.ClickPrevious();
		}

		[When(@"I click the OK button")]
		public void WhenIClickTheOKButton()
		{
			Pages.Pages.CurrentOkButton.OkButton.EventualClick();
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
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .icon-comment", formattedDate));
		}

		[Then(@"I should see an overtime availability symbol with tooltip")]
		public void ThenIShouldSeeAnOvertimeAvailabilitySymbolWithTooltip(Table table)
		{
			var overtimeAvailability = table.CreateInstance<OvertimeAvailabilityTooltipAndBar>();
			var formattedDate = overtimeAvailability.Date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertVisibleUsingJQuery(string.Format("ul.weekview-day[data-mytime-date={0}] li .overtime-availability-symbol", formattedDate));
			Browser.Interactions.AssertKnockoutContextContains(string.Format("ul.weekview-day[data-mytime-date={0}]", formattedDate), "textOvertimeAvailabilityText()", overtimeAvailability.StartTime);
			Browser.Interactions.AssertKnockoutContextContains(string.Format("ul.weekview-day[data-mytime-date={0}]", formattedDate), "textOvertimeAvailabilityText()", overtimeAvailability.EndTime);
		}

		[Then(@"I should not see a symbol at the top of the schedule for date '(.*)'")]
		public void ThenIShouldNotSeeASymbolAtTheTopOfTheScheduleForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format(".weekview-day[data-mytime-date={0}] .icon-comment", formattedDate));
		}

		[Then(@"I should not see an overtime availability symbol for date '(.*)'")]
		public void ThenIShouldNotSeeAnOvertimeAvailabilitySymbolForDate(DateTime date)
		{
			var formattedDate = date.ToString(CultureInfo.GetCultureInfo("sv-SE").DateTimeFormat.ShortDatePattern);
			Browser.Interactions.AssertNotVisibleUsingJQuery(string.Format("ul.weekview-day[data-mytime-date={0}] li .overtime-availability-symbol", formattedDate));
		}

		[Then(@"I should see an indication that no agents that can go on holiday for date '(.*)'")]
		public void ThenIShouldSeeAnIndicationThatNoAgentsThatCanGoOnHolidayForDate(DateTime date)
		{
			var holidayAgentsSymbol = Pages.Pages.WeekSchedulePage.HolidayAgentsForDate(date);
			EventualAssert.That(() => holidayAgentsSymbol.DisplayHidden(), Is.True);
		}

		[Then(@"I should see current or first future virtual schedule period \+/- 1 week")]
		public void ThenIShouldSeeCurrentOrFirstFutureVirtualSchedulePeriod_1Week()
		{
			var virtualSchedulePeriodData = DataMaker.Data().UserData<SchedulePeriod>();

			var firstDateDisplayed = virtualSchedulePeriodData.FirstDayOfDisplayedPeriod();
			var lastDateDisplayed = virtualSchedulePeriodData.LastDayOfDisplayedPeriod();
			calendarShouldRangeBetween(firstDateDisplayed, lastDateDisplayed);
		}

		[Then(@"I should see a user-friendly message explaining I dont have anything to view")]
		public void ThenIShouldSeeAUser_FriendlyMessageExplainingIDontHaveAnythingToView()
		{
			Browser.Interactions.AssertExists(".alert.alert-block");
		}

		[Then(@"I should see next virtual schedule period")]
		public void ThenIShouldSeeNextVirtualSchedulePeriod()
		{
			var virtualSchedulePeriodData = DataMaker.Data().UserData<SchedulePeriod>();

			var nextPeriodFirstDateDisplayed = virtualSchedulePeriodData.FirstDayOfNextDisplayedVirtualSchedulePeriod();
			var nextPeriodLastDateDisplayed = virtualSchedulePeriodData.LastDayOfNextDisplayedVirtualSchedulePeriod();
			calendarShouldRangeBetween(nextPeriodFirstDateDisplayed, nextPeriodLastDateDisplayed);
		}

		[Then(@"I should see previous virtual schedule period")]
		public void ThenIShouldSeePreviousVirtualSchedulePeriod()
		{
			var virtualSchedulePeriodData = DataMaker.Data().UserData<SchedulePeriod>();

			var previousPeriodFirstDateDisplayed = virtualSchedulePeriodData.FirstDayOfDisplayedPreviousVirtualSchedulePeriod();
			var previousPeriodLastDateDisplayed = virtualSchedulePeriodData.LastDayOfDisplayedPreviousVirtualSchedulePeriod();
			calendarShouldRangeBetween(previousPeriodFirstDateDisplayed, previousPeriodLastDateDisplayed);
		}

		private static void calendarShouldRangeBetween(DateTime firstDateDisplayed, DateTime lastDateDisplayed)
		{
			var page = Browser.Current.Page<CalendarCellsPage>();
			EventualAssert.That(() => page.FirstCalendarCellDate, Is.EqualTo(firstDateDisplayed.ToString("yyyy-MM-dd")));
			EventualAssert.That(() => page.LastCalendarCellDate, Is.EqualTo(lastDateDisplayed.ToString("yyyy-MM-dd")));
		}

		[Then(@"I should see a message saying I am missing a workflow control set")]
		public void ThenIShouldSeeAMessageSayingIAmMissingAWorkflowControlSet()
		{
			var page = Browser.Current.Page<CalendarCellsPage>();
			if (page is StudentAvailabilityPage)
				((StudentAvailabilityPage) page).StudentAvailabilityPeriod.InnerHtml.Should().Contain(Resources.MissingWorkflowControlSet);
			if (page is PreferencePage)
				((PreferencePage)page).PreferencePeriod.InnerHtml.Should().Contain(Resources.MissingWorkflowControlSet);
		}
	}
}
