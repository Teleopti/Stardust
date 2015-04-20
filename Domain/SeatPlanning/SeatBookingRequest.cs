using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequest
	{
		private readonly ISeatBooking[] _seatBookings;

		public SeatBookingRequest(params ISeatBooking[] seatBookings)
		{
			_seatBookings = seatBookings;
		}

		public IEnumerable<ISeatBooking> SeatBookings { get { return _seatBookings; } }

		public int MemberCount { get { return _seatBookings.Length; } }
		
	}
}
