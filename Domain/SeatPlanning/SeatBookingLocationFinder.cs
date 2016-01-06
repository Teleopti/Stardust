using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public static class SeatBookingLocationFinder
	{
		public static SeatMapLocation GetLocationForBooking(IList<SeatMapLocation> seatMapLocations, ISeatBooking seatBooking,  List<SeatScore> seatScores)
		{
			var locationsInOrder = from location in seatMapLocations
				where location.IncludeInSeatPlan
				orderby
					getAgentRoleAndSeatRoleIntersectionCount(location.Seats, seatBooking) descending,
					location.SeatCount descending
				select location;
			
			return locationsInOrder.FirstOrDefault(location => canAllocateShifts(location, seatBooking, seatScores));
		}

		private static bool canAllocateShifts(ISeatMapLocation location, ISeatBooking seatBooking, List<SeatScore> seatScores)
		{
			if (!location.IncludeInSeatPlan)
			{
				return false;
			}

			var transientSeatBookings = location.Seats.Select(s => new TransientSeatBooking(s)).ToList();
			
			var seat = SeatBookingSeatFinder.TryToFindASeatForBookingCheckingTransientBookings(seatBooking, location.Seats, transientSeatBookings, seatScores);
			if (seat != null)
			{
				var transientSeatBooking = transientSeatBookings.SingleOrDefault (booking => booking.Seat == seat);
				if (transientSeatBooking != null && !transientSeatBooking.IsAllocated (seatBooking))
				{
					transientSeatBooking.TemporarilyAllocate (seatBooking);
					return true;
				}
			}
			
			return false;
		}

		private static int getAgentRoleAndSeatRoleIntersectionCount(IEnumerable<ISeat> seats, ISeatBooking seatBooking)
		{
			return (from seat in seats
				let personApplicationRoles = seatBooking.Person.PermissionInformation.ApplicationRoleCollection
				let intersectionCount = seat.Roles.Intersect(personApplicationRoles).Count()
				where seat.Roles.Count == intersectionCount
				select intersectionCount).Sum();
		}
		
	}
	
}