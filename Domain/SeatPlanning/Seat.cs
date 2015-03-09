using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class Seat : AggregateEntity
	{
		public virtual String Name { get; set; }
		public virtual int Priority { get; set; }
		public virtual Location Location { get; set; }

		public Seat() { }

		public Seat(string name, int priority)
		{
			// need Id of the seat before persistance, to update seat map blob.
			SetId (Guid.NewGuid()); 

			Name = name;
			Priority = priority;
		}

		private readonly List<BookingPeriod> _bookingPeriods = new List<BookingPeriod>();

		public virtual void Book(BookingPeriod bookingPeriod)
		{
			_bookingPeriods.Add(bookingPeriod);
		}

		public virtual bool IsAllocated(BookingPeriod bookingPeriodRequest)
		{
			return _bookingPeriods.Any (bookingPeriod => bookingPeriod.Intersects (bookingPeriodRequest));
		}

		public virtual void ClearBookings()
		{
			_bookingPeriods.Clear();
		}
	}
}
