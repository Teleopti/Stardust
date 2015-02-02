using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class Location
	{
		private readonly List<Seat> _seats = new List<Seat>();
		private readonly List<Location> _childLocations = new List<Location>();


		public Guid Id { get; set; }
		public String Name { get; set; }
		public bool IncludeInSeatPlan { get; set; }

		public void AddChild(Location childLocation)
		{
			_childLocations.Add(childLocation);
		}

		public void AddChildren(IEnumerable<Location> childLocations)
		{
			if (childLocations != null)
			{
				_childLocations.AddRange(childLocations);
			}
		}

		public void AddSeat(Guid id, String name)
		{
			_seats.Add(new Seat(id, name));
		}

		public void AddSeats(IEnumerable<Seat> seats)
		{
			if (seats != null)
			{
				_seats.AddRange(seats);
			}
		}

		public void ClearBookingInformation()
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

		public Seat GetNextUnallocatedSeat(BookingPeriod period, Boolean ignoreChildren)
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
				return GetNextUnallocatedSeatOnSelf (period);
			}
			
			return null;
		}

		private Seat GetNextUnallocatedSeatOnSelf(BookingPeriod period)
		{
			return _seats.FirstOrDefault(seat => !seat.IsAllocated(period));
		}

		public int SeatCount { get { return _seats.Count; } }

		public bool CanAllocateShifts(IEnumerable<AgentShift> agentShifts)
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


		public Location GetLocationToAllocateSeats(IEnumerable<AgentShift> agentShifts)
		{
			foreach (var childLocation in _childLocations.OrderByDescending(l => l.SeatCount))
			{
				//if (childLocation.CanAllocateShifts(agentShifts))
				//{
				//	return childLocation;
				//}

				var location = childLocation.GetLocationToAllocateSeats (agentShifts);
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
