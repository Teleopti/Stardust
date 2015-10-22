﻿using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public static class SeatPlannerHelper
	{
		public static void AttachExistingSeatBookingsToSeats(ISeatMapLocation rootSeatMapLocation, IList<ISeatBooking> existingSeatBookings)
		{
			AttachExistingSeatBookingsToSeats(rootSeatMapLocation.Seats, existingSeatBookings);
			rootSeatMapLocation
				.ChildLocations
				.ForEach(location => AttachExistingSeatBookingsToSeats(location, existingSeatBookings));
		}

		public static void AttachExistingSeatBookingsToSeats(IEnumerable<ISeat> seats, IList<ISeatBooking> existingSeatBookings)
		{
			foreach (var seat in seats)
			{
				var seatBookings = existingSeatBookings.Where(booking => Equals(booking.Seat, seat)).ToList();
				seat.AddSeatBookings(seatBookings);
			}
		}
	}
}