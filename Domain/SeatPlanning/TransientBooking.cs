using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class TransientBooking
	{
		private readonly ISeat _seat;
		private readonly ICollection<ISeatBooking> _almostAllocatedBookings = new Collection<ISeatBooking>();

		public ISeat Seat
		{
			get { return _seat; }
		}

		public TransientBooking(ISeat seat)
		{
			_seat = seat;
		}

		public bool IsAllocated(ISeatBooking booking)
		{
			return _seat.IsAllocated(booking) || _almostAllocatedBookings.Any(p => p.Intersects(booking));
		}

		public void TemporarilyAllocate(ISeatBooking booking)
		{
			_almostAllocatedBookings.Add(booking);
		}
	}
}
