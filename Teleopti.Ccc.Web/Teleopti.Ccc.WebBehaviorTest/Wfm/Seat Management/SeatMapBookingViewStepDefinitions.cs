using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

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
			Browser.Interactions.HoverOver("seatmap-occupancy-detail .wfm-list li");
			Browser.Interactions.Click("seatmap-occupancy-detail .wfm-leave-behind span");
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

	}
}
