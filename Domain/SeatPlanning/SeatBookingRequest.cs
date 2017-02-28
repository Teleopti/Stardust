using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequest
	{
		private readonly IList<ISeatBooking> _seatBookings;

		public SeatBookingRequest(params ISeatBooking[] seatBookings)
		{
			_seatBookings = seatBookings.ToList();
		}

		public IEnumerable<ISeatBooking> SeatBookings { get { return _seatBookings; } }
		
	}
}
