﻿using System;
using Teleopti.Ccc.WebBehaviorTest.Core;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Seat_Management
{
	[Binding]
	internal class SeatPlanReportStepDefinitions
	{

		[When(@"I click SeatPlanReport button")]
		public void WhenIClickSeatPlanReportButton()
		{
			Browser.Interactions.Click(".planningDayDetail button");
		}

		[Then(@"I should be able to view the report content")]
		public void ThenIShouldBeAbleToViewTheReportContent()
		{
			Browser.Interactions.AssertExists("seat-plan-report-table table");
		}

		[Then(@"paging bar should be hide")]
		public void ThenPagingBarShouldBeHide()
		{
			Browser.Interactions.AssertNotExists("seat-plan-report-table table", ".report-paging");
		}

		[When(@"I click SeatPlanReport button from planning period")]
		public void WhenIClickSeatPlanReportButtonFromPlanningPeriod()
		{
			Browser.Interactions.Click("wfm-card-list card-header");
			Browser.Interactions.Click("wfm-card-list card-body .period-report-btn");
		}

		[Then(@"I should be able to view the report from ""(.*)"" to ""(.*)""")]
		public void ThenIShouldBeAbleToViewTheReportFromTo(DateTime fromDate, DateTime toDate)
		{
			Browser.Interactions.AssertAnyContains("seat-plan-report-table table", fromDate.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertAnyContains("seat-plan-report-table table", toDate.ToString("yyyy-MM-dd"));
		}

	}
}
