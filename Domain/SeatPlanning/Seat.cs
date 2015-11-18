using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	

	[Serializable]
	public class Seat : AggregateEntity, ISeat
	{
		private IList<IApplicationRole> _roles = new List<IApplicationRole>();

		public virtual String Name { get; set; }
		public virtual int Priority { get; set; }

		public Seat() { }

		public Seat(string name, int priority)
		{
			// need Id of the seat before persistance, to update seat map slob.
			SetId (Guid.NewGuid()); 

			Name = name;
			Priority = priority;
		}

		public virtual IList<IApplicationRole> Roles
		{
			get { return _roles; }
		}

		private readonly List<ISeatBooking> _seatBookings = new List<ISeatBooking>();

		public virtual void AddSeatBooking(ISeatBooking seatBooking)
		{
			_seatBookings.Add(seatBooking);
		}

		public virtual void AddSeatBookings(IList<ISeatBooking> seatBookings)
		{
			_seatBookings.AddRange(seatBookings);
		}

		public virtual void RemoveSeatBooking (ISeatBooking seatBooking)
		{
			_seatBookings.Remove (seatBooking);
		}

		public virtual bool IsAllocated(ISeatBooking seatBookingRequest)
		{
			return _seatBookings.Any (bookingPeriod => bookingPeriod.Intersects (seatBookingRequest));
		}

		public virtual void ClearBookings()
		{
			_seatBookings.Clear();
		}

		public virtual void AddRoles(params IApplicationRole[] roles)
		{
			roles.ForEach(_roles.Add);
		}
	}
}
