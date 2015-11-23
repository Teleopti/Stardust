using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatAllocatorHelper
	{
		public static IOrderedEnumerable<SeatBookingRequest> SortSeatBookingRequests(SeatBookingRequest[] seatBookingRequests)
		{
			return seatBookingRequests
				.OrderByDescending(s => s.MemberCount)
				.ThenBy(s => s.SeatBookings.Min(booking => booking.StartDateTime));
		}
		
		public static ISeat TryToFindASeatForBooking(ISeatBooking seatBooking, IEnumerable<ISeat> seats)
		{
			var seatsByPersonRoleMatch = findPossibleSeatsRankedByPersonRoleMatches(seatBooking, seats);
			return seatsByPersonRoleMatch.FirstOrDefault((seat) => !seat.IsAllocated(seatBooking));
		}
		
		private static IEnumerable<ISeat> findPossibleSeatsRankedByPersonRoleMatches(ISeatBooking seatBooking, IEnumerable<ISeat> seats)
		{
			var possibleSeatsRankedByNumberOfMatchingRoles = from seat in seats
				let personApplicationRoles = seatBooking.Person.PermissionInformation.ApplicationRoleCollection
				let intersectionCount = seat.Roles.Intersect (personApplicationRoles).Count()
				where seat.Roles.Count == intersectionCount
				orderby intersectionCount descending
				select seat;

			return possibleSeatsRankedByNumberOfMatchingRoles;
		}
	}
}