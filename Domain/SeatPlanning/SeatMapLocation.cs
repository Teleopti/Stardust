using System;
using System.Collections;
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
		private IList<ISeat> _seats = new List<ISeat>();
		private IList<SeatMapLocation> _childLocations = new List<SeatMapLocation>();

		public virtual IList<ISeat> Seats
		{
			get { return _seats; }
		}

		public virtual IList<SeatMapLocation> ChildLocations
		{
			get { return _childLocations; }

		}

		public virtual string Name { get; set; }
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
			AddChild(childSeatMapLocation);
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
			var seat = new Seat(name, priority);
			_seats.Add(seat);
			seat.SetParent(this);
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


		public virtual IList<SeatMapLocation> GetFullLocationHierachyAsList()
		{
			var locations = new List<SeatMapLocation> ();

			foreach (var child in _childLocations)
			{
				locations.AddRange(child.GetFullLocationHierachyAsList().ToList());
			}
			
			locations.Add (this);

			return locations;
		}


		public virtual void UpdateSeatMapTemporaryId(Guid? temporaryId, Guid? persistedId)
		{
			SeatMapJsonData = SeatMapJsonData.Replace(temporaryId.ToString(), persistedId.ToString());
		}

		public virtual ISeat GetNextUnallocatedSeat(ISeatBooking booking)
		{
			if (IncludeInSeatPlan)
			{
				return SeatAllocatorHelper.TryToFindASeatForBooking(booking, _seats.OrderBy(seat => seat.Priority));
			}

			return null;
		}


		public virtual int SeatCount { get { return _seats.Count; } }

		public virtual bool CanAllocateShifts(params ISeatBooking[] agentShifts)
		{
			if (!IncludeInSeatPlan)
			{
				return false;
			}

			var transientSeatBookings = _seats.Select(s => new TransientBooking(s)).ToList();
			var temporaryBookings = agentShifts.Select(s => new TemporaryBooking(s)).ToList();

			foreach (var agentShift in temporaryBookings)
			{
				var seat = SeatAllocatorHelper.TryToFindASeatForBookingCheckingTransientBookings (agentShift.SeatBooking, _seats, transientSeatBookings);

				if (seat != null)
				{
					var temporarySeatBooking = transientSeatBookings.SingleOrDefault (booking => booking.Seat == seat);

					if (temporarySeatBooking != null && !temporarySeatBooking.IsAllocated (agentShift.SeatBooking))
					{
						temporarySeatBooking.TemporarilyAllocate (agentShift.SeatBooking);
						agentShift.SetBooked();

					}
				}
			}

			
			return temporaryBookings.All(s => s.IsBooked);
		}

		private class TemporaryBooking
		{
			private readonly ISeatBooking _seatBooking;
			private bool _isBooked;

			public TemporaryBooking(ISeatBooking seatBooking)
			{
				_seatBooking = seatBooking;
			}

			public ISeatBooking SeatBooking
			{
				get { return _seatBooking; }
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
