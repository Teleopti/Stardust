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

		private class memberCountStartDateComparer : IComparer<SeatBookingRequest>
		{
			public int Compare(SeatBookingRequest seatBookingRequest1, SeatBookingRequest seatBookingRequest2)
			{

				if (seatBookingRequest1 == seatBookingRequest2)
				{
					return 0;
				}

				var request1SeatBookingCount = seatBookingRequest1._seatBookings.Count;
				var request2SeatBookingCount = seatBookingRequest2._seatBookings.Count;

				if (request1SeatBookingCount < request2SeatBookingCount)
					return 1;

				if (request1SeatBookingCount > request2SeatBookingCount)
					return -1;
				

				//temporary remove for diagnostics
				//var miniumBookingStartDateRequest1 = seatBookingRequest1.SeatBookings.Min(booking => booking.StartDateTime);
				//var miniumBookingStartDateRequest2 = seatBookingRequest2.SeatBookings.Min(booking => booking.StartDateTime);

				//if (miniumBookingStartDateRequest1 < miniumBookingStartDateRequest2)
				//{
				//	return -1;
				//}

				//if (miniumBookingStartDateRequest1 > miniumBookingStartDateRequest2)
				//{
				//	return 1;
				//}

				return 0;
			}
		}

		
	}
}
