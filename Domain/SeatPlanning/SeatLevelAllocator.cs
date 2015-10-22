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

			var sortedSeatBookingRequests = seatBookingRequests
				.OrderByDescending(s => s.MemberCount)
				.ThenBy(s => s.SeatBookings.Min(booking => booking.StartDateTime));


			foreach (var seatBookingRequest in sortedSeatBookingRequests)
			{
				seatBookingRequest.SeatBookings.OrderBy(booking => booking.StartDateTime).ForEach(tryToFindASeatForBooking);
			}
		}

		private void tryToFindASeatForBooking(ISeatBooking seatBooking)
		{
			var unallocatedSeat = _seats.FirstOrDefault(seat => !seat.IsAllocated(seatBooking));

			if (unallocatedSeat != null)
			{
				seatBooking.Book(unallocatedSeat);
			}

		}

	}
}