using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	
	[Serializable]
	public class SeatMapLocation : VersionedAggregateRootWithBusinessUnit, ISeatMapLocation
	{
		private IList<Seat> _seats = new List<Seat>();
		private IList<SeatMapLocation> _childLocations = new List<SeatMapLocation>();

		public virtual IList<Seat> Seats
		{
			get { return _seats; }
		}

		public virtual IList<SeatMapLocation> ChildLocations
		{
			get { return _childLocations; }
		}

		public virtual String Name { get; set; }
		public virtual bool IncludeInSeatPlan { get; set; }
		public virtual SeatMapLocation ParentLocation { get; set; }
		public virtual string SeatMapJsonData { get; set; }

		public SeatMapLocation()
		{
			
		}

		public virtual void SetLocation(String seatMapJsonData, String name)
		{
			SeatMapJsonData = seatMapJsonData;
			Name = name;
		}

		public virtual SeatMapLocation CreateChildSeatMapLocation(LocationInfo location)
		{
			var childSeatMapLocation = new SeatMapLocation();
			childSeatMapLocation.SetLocation("{}", location.Name);
			AddChild (childSeatMapLocation);
			return childSeatMapLocation;
		}


		public virtual void AddChild(SeatMapLocation childSeatMapLocation)
		{
			_childLocations.Add(childSeatMapLocation);
			childSeatMapLocation.setParentLocation(this);
		}

		private void setParentLocation(SeatMapLocation seatMapLocation)
		{
			ParentLocation = seatMapLocation;
		}

		public virtual void AddChildren(IEnumerable<SeatMapLocation> childLocations)
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

		public virtual void UpdateSeatMapTemporaryId(Guid? temporaryId, Guid? persistedId)
		{
			SeatMapJsonData = SeatMapJsonData.Replace(temporaryId.ToString(), persistedId.ToString());
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


		public virtual SeatMapLocation GetLocationToAllocateSeats(IEnumerable<AgentShift> agentShifts)
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
