using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class TransientSeatBooking
	{
		private readonly ISeat _seat;
		private readonly ICollection<ISeatBooking> _almostAllocatedBookings = new Collection<ISeatBooking>();

		public ICollection<ISeatBooking> AlmostAllocatedBookings
		{
			get { return _almostAllocatedBookings; }
		}

		public ISeat Seat
		{
			get { return _seat; }
		}

		public TransientSeatBooking(ISeat seat)
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
