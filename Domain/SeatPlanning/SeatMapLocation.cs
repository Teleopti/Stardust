using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	[Serializable]
	public class SeatMapLocation : AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit, ISeatMapLocation
	{
		private readonly IList<ISeat> _seats = new List<ISeat>();
		private readonly IList<SeatMapLocation> _childLocations = new List<SeatMapLocation>();

		public virtual IList<ISeat> Seats => _seats;

		public virtual IList<SeatMapLocation> ChildLocations => _childLocations;

		public virtual string Name { get; set; }
		public virtual string LocationPrefix { get; set; }
		public virtual string LocationSuffix { get; set; }
		public virtual bool IncludeInSeatPlan { get; set; }
		public virtual SeatMapLocation ParentLocation { get; set; }
		public virtual string SeatMapJsonData { get; set; }

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
				seat.SetParent (this);
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

		public virtual IList<SeatMapLocation> GetFullLocationHierarchyAsList()
		{
			var locations = new List<SeatMapLocation> ();

			foreach (var child in _childLocations)
			{
				locations.AddRange(child.GetFullLocationHierarchyAsList().ToList());
			}
			
			locations.Add (this);

			return locations;
		}
		
		public virtual void UpdateSeatMapTemporaryId(Guid? temporaryId, Guid? persistedId)
		{
			SeatMapJsonData = SeatMapJsonData.Replace(temporaryId.ToString(), persistedId.ToString());
		}
		
		public virtual int SeatCount => _seats.Count;
	}
}
