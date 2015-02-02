using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class Seat
	{
		
		private readonly List<BookingPeriod> _bookingPeriods = new List<BookingPeriod>();

		public Seat(Guid id, String name)
		{
			Name = name;
			Id = id;
		}

		public Guid Id { get; set; }
		public String Name { get; set; }

		public void Book(BookingPeriod bookingPeriod)
		{
			_bookingPeriods.Add(bookingPeriod);
		}

		public bool IsAllocated(BookingPeriod bookingPeriodRequest)
		{
			foreach (var bookingPeriod in _bookingPeriods)
			{
				if (bookingPeriod.Intersects(bookingPeriodRequest))
					return true;
			}
			return false;
		}

		public void ClearBookings()
		{
			_bookingPeriods.Clear();
		}
	}
}
