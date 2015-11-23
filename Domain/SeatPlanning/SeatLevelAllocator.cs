using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatLevelAllocator
	{
		private readonly IList<ISeat> _seats;

		public SeatLevelAllocator(IEnumerable<ISeat> seats)
		{
			_seats = seats.OrderBy(seat => seat.Priority).ToList();
		}

		public void AllocateSeats(params SeatBookingRequest[] seatBookingRequests)
		{
			var sortedSeatBookingRequests = SeatAllocatorHelper.SortSeatBookingRequests (seatBookingRequests);

			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				seatBookingRequest.SeatBookings.OrderBy(booking => booking.StartDateTime).ForEach(bookSeatIfAvailable);
			}
		}

		private void bookSeatIfAvailable(ISeatBooking seatBooking)
		{
			var firstApplicableSeat = SeatAllocatorHelper.TryToFindASeatForBooking (seatBooking, _seats);
			if (firstApplicableSeat != null )
			{
				seatBooking.Book(firstApplicableSeat);
			}
		}
	}
}