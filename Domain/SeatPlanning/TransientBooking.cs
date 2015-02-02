using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class TransientBooking
	{
		private readonly Seat _seat;
		private readonly ICollection<BookingPeriod> _almostAllocatedPeriods = new Collection<BookingPeriod>();

		public TransientBooking(Seat seat)
		{
			_seat = seat;
		}

		public bool IsAllocated(BookingPeriod period)
		{
			return _seat.IsAllocated(period) || _almostAllocatedPeriods.Any(p => p.Intersects(period));
		}

		public void TemporarilyAllocate(BookingPeriod period)
		{
			_almostAllocatedPeriods.Add(period);
		}
	}
}
