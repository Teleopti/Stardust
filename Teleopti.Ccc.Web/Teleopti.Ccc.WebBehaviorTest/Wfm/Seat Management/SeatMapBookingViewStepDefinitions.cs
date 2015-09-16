using TechTalk.SpecFlow;
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
			Browser.Interactions.HoverOver("seatmap-occupancy-detail .wfm-list li");
			Browser.Interactions.Click("seatmap-occupancy-detail .wfm-leave-behind span");
		}

	}
}
