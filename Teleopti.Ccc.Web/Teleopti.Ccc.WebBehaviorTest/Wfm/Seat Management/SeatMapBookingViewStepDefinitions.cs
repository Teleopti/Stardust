using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Seat_Management
{
	[Binding]
	internal class SeatMapBookingViewStepDefinitions
	{
		[When(@"I press seat map booking view button")]
		public void WhenIPressSeatMapBookingViewButton()
		{
			Browser.Interactions.Click(".planning-day-detail .wfm-btn-invis-primary");
		}

		[Then(@"the seat map booking should be opened")]
		public void ThenTheSeatMapBookingShouldBeOpened()
		{
			Browser.Interactions.AssertExists(".seatmap");
		}

		[When(@"I press back button on seat booking view")]
		public void WhenIPressBackButtonOnSeatBookingView()
		{
			Browser.Interactions.Click(".seatmap-view-header .return-button");
		}

		[Then(@"I should go back to seat plan")]
		public void ThenIShouldGoBackToSeatPlan()
		{
			Browser.Interactions.AssertExists("#seatplan .planning-period-title");
		}

		[Then(@"the date of datepicker should be correct")]
		public void ThenTheDateOfDatepickerShouldBeCorrect()
		{
			Browser.Interactions.AssertAnyContains(".seatmap-view-datepicker", "2015-01-01");
		}

		[Then(@"I am able to view booking detail of selected seat")]
		public void ThenIAmAbleToViewBookingDetailOfSelectedSeat()
		{
			Browser.Interactions.AssertExists(".seatmap-occupancy-detail");
		}

		[Then(@"I delete the first record under the seat booking details")]
		public void ThenIDeleteTheFirstRecordUnderTheSeatBookingDetails()
		{
			Browser.Interactions.HoverOver(".seatmap-occupancy-detail .seat-booking-list .wfm-list li");
			Browser.Interactions.Click(".seatmap-occupancy-detail .wfm-leave-behind span");
		}

		[When(@"I click add agents to seat button")]
		public void WhenIClickAddAgentsToSeatButton()
		{
			Browser.Interactions.Click(".seatbooking-operations-add .add-agents");
		}

		[Then(@"I should see people search list")]
		public void ThenIShouldSeePeopleSearchList()
		{
			Browser.Interactions.AssertExists(".people-search");
			Browser.Interactions.AssertExists(".people-list");
		}

		[When(@"I click cancel button after open people search list")]
		public void WhenIClickCancelButtonAfterOpenPeopleSearchList()
		{
			Browser.Interactions.Click(".seatbooking-operations-cancel .cancel-operation");
		}

		[Then(@"I should not see people search list")]
		public void ThenIShouldNotSeePeopleSearchList()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery(".seatmap-occupancy-detail .people-search");
			Browser.Interactions.AssertNotVisibleUsingJQuery(".seatmap-occupancy-detail .people-list");
		}

		[Then(@"I should not see the action buttons of people")]
		public void ThenIShouldNotSeeTheActionButtonsOfPeople()
		{
			Browser.Interactions.AssertNotExists(".seatmap-occupancy-detail", ".seatmap-occupancy-detail .action-panel");
		}

		[When(@"I select agent '(.*)' from search list")]
		public void WhenISelectAgentFromSearchList(string agent)
		{
			Browser.Interactions.AssertAnyContains("people-selection-list .ui-grid-canvas>.ui-grid-row:first-child",agent);
			Browser.Interactions.Click("people-selection-list .ui-grid-canvas>.ui-grid-row:first-child");
		}

		[When(@"I click assign button")]
		public void WhenIClickAssignButton()
		{
			Browser.Interactions.Click(".seatbooking-operations-cancel .assign-agents");
		}

		[Then(@"I should see '(.*)' in the result of seat plan")]
		public void ThenIShouldSeeInTheResultOfSeatPlan(string agent)
		{
			Browser.Interactions.AssertAnyContains(".seatmap-occupancy-detail", agent);
		}

		[When(@"I select first '(.*)' seats")]
		public void WhenISelectFirstSeats(int seatNumber)
		{
			Browser.Interactions.AssertExists(".seat-booking-list");

			var selectSeats = @"utils.selectMultipleSeatsForScenarioTest(canvas," + seatNumber + ")";

			executedJavascript(selectSeats);
		}


		[Then(@"I should see occupancy detail of two seats in occupancy detail panel")]
		public void ThenIShouldSeeOccupancyDetailOfTwoSeatsInOccupancyDetailPanel()
		{
			Browser.Interactions.AssertExists(".seat-booking-list");
			Browser.Interactions.AssertAnyContains(".seat-booking-list", "Seat: 1");
			Browser.Interactions.AssertAnyContains(".seat-booking-list", "Seat: 2");
		}

		[Then(@"I should see occupancy detail of three seats in occupancy detail panel")]
		public void ThenIShouldSeeOccupancyDetailOfThreeSeatsInOccupancyDetailPanel()
		{
			Browser.Interactions.AssertExists(".seat-booking-list");
			Browser.Interactions.AssertAnyContains(".seat-booking-list", "Seat: 1");
			Browser.Interactions.AssertAnyContains(".seat-booking-list", "Seat: 2");
			Browser.Interactions.AssertAnyContains(".seat-booking-list", "Seat: 3");
		}


		[Then(@"I should not see any seat booking details")]
		public void ThenIShouldNotSeeAnySeatBookingDetails()
		{
			Browser.Interactions.AssertExists(".seat-booking-list");
			Browser.Interactions.AssertNoContains(".seat-booking-list", ".seat-booking-item", "I");
		}


		private void executedJavascript(string function)
		{
			var javascript = @"var injector = angular.element(document.getElementsByClassName('ng-scope')[0]).injector(); " +
							 @"var utils = injector.get('seatMapCanvasUtilsService');" +
							 @"var canvas = angular.element(document.getElementsByClassName('seatmap')).scope().vm.getCanvas();;" +
							 function;
			Browser.Interactions.Javascript(javascript);
		}
		
	}
}
