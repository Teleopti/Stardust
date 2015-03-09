using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class Location : AggregateEntity
	{
		private IList<Seat> _seats = new List<Seat>();
		private IList<Location> _childLocations = new List<Location>();

		public virtual IList<Seat> Seats
		{
			get { return _seats; }
		}

		public virtual IList<Location> ChildLocations
		{
			get { return _childLocations; }
		}

		public virtual String Name { get; set; }
		public virtual bool IncludeInSeatPlan { get; set; }
		public virtual Location ParentLocation { get; set; }

		public virtual void AddChild(Location childLocation)
		{
			_childLocations.Add(childLocation);
			childLocation.setParentLocation(this);
		}

		private void setParentLocation(Location location)
		{
			ParentLocation = location;
		}

		public virtual void AddChildren(IEnumerable<Location> childLocations)
		{
			foreach (var child in childLocations)
			{
				_childLocations.Add(child);
				child.setParentLocation(this);
			}
		}

		public virtual bool HasChild(Guid id)
		{
			return _childLocations.Any(loc => loc.Id == id);
		}

		public virtual Seat AddSeat(String name, int priority)
		{
			var seat = new Seat (name, priority);
			_seats.Add(seat);
			seat.SetParent (this);
			return seat;
		}

		public virtual void AddSeats(IEnumerable<Seat> seats)
		{
			if (seats == null) return;

			foreach (var seat in seats)
			{
				_seats.Add(seat);
			}
		}

		public virtual void ClearBookingInformation()
		{
			foreach (var seat in _seats)
			{
				seat.ClearBookings();
			}

			foreach (var child in _childLocations)
			{
				child.ClearBookingInformation();
			}
		}

		public virtual Seat GetNextUnallocatedSeat(BookingPeriod period, Boolean ignoreChildren)
		{
			if (!ignoreChildren)
			{
				foreach (var seat in _childLocations.OrderByDescending(l => l.SeatCount)
					.Select(childLocation => childLocation.GetNextUnallocatedSeat(period, false))
					.Where(seat => seat != null))
				{
					return seat;
				}
			}

			if (IncludeInSeatPlan)
			{
				return getNextUnallocatedSeatOnSelf(period);
			}

			return null;
		}

		private Seat getNextUnallocatedSeatOnSelf(BookingPeriod period)
		{
			return _seats.OrderBy(seat => seat.Priority).FirstOrDefault(seat => !seat.IsAllocated(period));
		}

		public virtual int SeatCount { get { return _seats.Count; } }

		public virtual bool CanAllocateShifts(IEnumerable<AgentShift> agentShifts)
		{
			if (!IncludeInSeatPlan || SeatCount < agentShifts.Count())
			{
				return false;
			}

			var temporaryBookedSeats = _seats.Select(s => new TransientBooking(s)).ToArray();
			var temporaryBookings = agentShifts.Select(s => new TemporaryBooking(s)).ToArray();
			foreach (var agentShift in temporaryBookings)
			{
				foreach (var temporaryBooking in temporaryBookedSeats)
				{
					if (!temporaryBooking.IsAllocated(agentShift.Shift.Period))
					{
						temporaryBooking.TemporarilyAllocate(agentShift.Shift.Period);
						agentShift.SetBooked();
						break;
					}
				}
			}
			return temporaryBookings.All(s => s.IsBooked);
		}


		public virtual Location GetLocationToAllocateSeats(IEnumerable<AgentShift> agentShifts)
		{
			foreach (var childLocation in _childLocations.OrderByDescending(l => l.SeatCount))
			{
				//if (childLocation.CanAllocateShifts(agentShifts))
				//{
				//	return childLocation;
				//}

				var location = childLocation.GetLocationToAllocateSeats(agentShifts);
				if (location != null)
				{
					return location;
				}
			}

			if (CanAllocateShifts(agentShifts))
			{
				return this;
			}

			return null;
		}

		private class TemporaryBooking
		{
			private readonly AgentShift _shift;
			private bool _isBooked;

			public TemporaryBooking(AgentShift shift)
			{
				_shift = shift;
			}

			public AgentShift Shift
			{
				get { return _shift; }
			}

			public bool IsBooked
			{
				get { return _isBooked; }
			}

			public void SetBooked()
			{
				_isBooked = true;
			}
		}


	}
}
