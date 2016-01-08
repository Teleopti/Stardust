using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatBookingRequest : IComparable<SeatBookingRequest>
	{
		private readonly IList<ISeatBooking> _seatBookings;

		public SeatBookingRequest(params ISeatBooking[] seatBookings)
		{
			_seatBookings = seatBookings.ToList();
		}

		public IEnumerable<ISeatBooking> SeatBookings { get { return _seatBookings; } }

		public int CompareTo (SeatBookingRequest other)
		{
			return  new memberCountStartDateComparer().Compare(this, other);
		}

		private class memberCountStartDateComparer : Comparer<SeatBookingRequest>
		{
			public override int Compare(SeatBookingRequest seatBookingRequest1, SeatBookingRequest seatBookingRequest2)
			{
				var seatBookingCountComparisonResult = seatBookingRequest1._seatBookings.Count.CompareTo (seatBookingRequest2._seatBookings.Count);

				if (seatBookingCountComparisonResult != 0)
					return -seatBookingCountComparisonResult;

				var miniumBookingStartDateRequest1 = seatBookingRequest1.SeatBookings.Min(request1Booking => request1Booking.StartDateTime);
				var miniumBookingStartDateRequest2 = seatBookingRequest2.SeatBookings.Min(request2Booking => request2Booking.StartDateTime);
				var miniumStartDateComparisonResult = miniumBookingStartDateRequest1.CompareTo (miniumBookingStartDateRequest2);

				if (miniumStartDateComparisonResult != 0)
					return miniumStartDateComparisonResult;
				

				return 0;
			}
		}

		
	}
}
