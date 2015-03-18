using System;

namespace Teleopti.Interfaces.Domain
{
	public interface ISeat : IAggregateEntity
	{
		String Name { get; set; }
		int Priority { get; set; }
		void AddSeatBooking(ISeatBooking seatBooking);
		bool IsAllocated(ISeatBooking seatBookingRequest);
		void ClearBookings();
	}
}