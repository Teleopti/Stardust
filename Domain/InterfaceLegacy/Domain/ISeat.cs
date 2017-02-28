using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface ISeat : IAggregateEntity
	{
		String Name { get; set; }
		int Priority { get; set; }
		IList<IApplicationRole> Roles { get; }
		void SetRoles(IList<IApplicationRole> roles);

		void AddSeatBooking(ISeatBooking seatBooking);
		void AddSeatBookings (IList<ISeatBooking> seatBookings);
		void RemoveSeatBooking (ISeatBooking seatBooking);
		bool IsAllocated(ISeatBooking seatBookingRequest);
		void ClearBookings();
	}
}