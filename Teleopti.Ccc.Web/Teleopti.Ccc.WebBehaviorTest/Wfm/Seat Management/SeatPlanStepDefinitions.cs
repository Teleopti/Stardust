using System;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Seat_Management
{
	[Binding]
	internal class SeatPlanStepDefinitions
	{
		[Given(@"there is a seatplan with")]
		public void GivenThereIsASeatPlanWith(Table table)
		{
			var seatPlan = table.CreateInstance<SeatPlanConfigurable>();
			DataMaker.Data().Apply(seatPlan);
		}

		[Given(@"there is a planning period with")]
		public void GivenThereIsAPlanningPeriodWith(Table table)
		{
			var planningPeriod = table.CreateInstance<PlanningPeriodConfigurable>();
			DataMaker.Data().Apply(planningPeriod);
		}

		[Given(@"there is a location with")]
		public void GivenThereIsALocationWith(Table table)
		{
			var planningPeriod = table.CreateInstance<SeatMapLocationConfigurable>();
			DataMaker.Data().Apply(planningPeriod);
		}

		[Given(@"there is a seat in root location with")]
		public void GivenThereIsASeatInRootLocationWith(Table table)
		{
			DataMaker.Data().Apply(table.CreateInstance<SeatConfigurable>());
		}

		[Given(@"there are '(.*)' more seats at root location")]
		public void GivenThereAreMoreSeatsAtRootLocation(int seatNumber)
		{
			DataMaker.Data().Apply(new SeatConfigurable(seatNumber));
		}


		[Given(@"there is a seat booking for me")]
		public void GivenThereIsASeatBookingForMe(Table table)
		{
			DataMaker.Data().Apply(table.CreateInstance<SeatBookingConfigurable>());
		}
		
		[Then(@"I should see planning period available for seat planning from '(.*)'to '(.*)'")]
		public void ThenIShouldSeeAPlanningPeriodAvailableForSeatPlanning(DateTime fromDate, DateTime toDate)
		{
			Browser.Interactions.AssertExists(".wfm-card-title");
			Browser.Interactions.AssertExists(".planning-period-header");

			Browser.Interactions.AssertAnyContains(".planning-period-header", fromDate.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertAnyContains(".planning-period-header", toDate.ToString("yyyy-MM-dd"));
		}

		[When(@"I choose the planning period beginning on '(.*)' for seat planning")]
		public void WhenIChooseThePlanningPeriodBeginningOnForSeatPlanning(DateTime date)
		{
			var headerSelector = string.Format(".planning-period-header[data-start-date=\"{0}\"]", date.ToString("yyyy-MM-dd"));
			Browser.Interactions.Click(headerSelector);
		}

		[Then(@"I should be able to initiate seat planning for location ""(.*)""")]
		public void ThenIShouldBeAbleToInitiateSeatPlanningForLocation(string locationName)
		{
			Browser.Interactions.AssertAnyContains(".wfm-btn", Resources.PlanSeats.ToUpper());
			Browser.Interactions.AssertExists(".angular-ui-tree-handle");
			Browser.Interactions.AssertAnyContains(".angular-ui-tree-handle", "BusinessUnit");
			Browser.Interactions.AssertAnyContains(".angular-ui-tree-handle", locationName);
		}

		[Then(@"I should see a Seat plan status of ""(.*)""")]
		public void ThenIShouldSeeASeatPlanStatusOf(string status)
		{
			Browser.Interactions.AssertAnyContains(".planning-day-detail p", status);
		}
	}
}