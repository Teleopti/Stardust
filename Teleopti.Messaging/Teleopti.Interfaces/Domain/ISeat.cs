using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeat : IAggregateEntity
	{
		String Name { get; set; }
		int Priority { get; set; }
		void AddSeatBooking(ISeatBooking seatBooking);
		void AddSeatBookings (IList<ISeatBooking> seatBookings);
		void RemoveSeatBooking (ISeatBooking seatBooking);
		bool IsAllocated(ISeatBooking seatBookingRequest);
		void ClearBookings();
	}
}