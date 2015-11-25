using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Optimization.IntraIntervalOptimization;
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

		public static ISeat TryToFindASeatForBookingCheckingTransientBookings(ISeatBooking seatBooking, IEnumerable<ISeat> seats, List<TransientBooking> transientBookings)
		{
			var seatsByPersonRoleMatch = findPossibleSeatsRankedByPersonRoleMatches(seatBooking, seats);
			
			var foundSeats = from seat in seatsByPersonRoleMatch 
				let temporarySeatBooking = transientBookings.SingleOrDefault (booking => booking.Seat == seat) 
				where temporarySeatBooking != null && 
						!temporarySeatBooking.IsAllocated (seatBooking) && 
						!seat.IsAllocated (seatBooking) select seat;
			
			return foundSeats.FirstOrDefault();
		}


		public static ISeat TryToFindASeatForBooking(ISeatBooking seatBooking, IEnumerable<ISeat> seats)
		{
			var seatsByPersonRoleMatch = findPossibleSeatsRankedByPersonRoleMatches(seatBooking, seats);
			return seatsByPersonRoleMatch.FirstOrDefault((seat) => !seat.IsAllocated(seatBooking));
		}

		public static IEnumerable<ISeatMapLocation> GetLocationsInOrderOfBookingOrder( IList<SeatMapLocation> seatMapLocations, params ISeatBooking[] seatBookings)
		{
			//if (bookGroupedRequestsTogether)
			//{
				var locationsInOrder = from location in seatMapLocations
									   where location.IncludeInSeatPlan
									   orderby
										   sumIntersectionsBetweenBookingRequestAgentRolesAndSeatRoles(seatBookings, location.Seats) descending,
										   location.SeatCount descending
									   select location;

				return locationsInOrder;
			//}

			//return seatMapLocations.OrderByDescending(location => location.SeatCount);
		}
		
		private static int sumIntersectionsBetweenBookingRequestAgentRolesAndSeatRoles(IEnumerable<ISeatBooking>seatBookings, IEnumerable<ISeat> seats)
		{
			return seatBookings.Sum (booking => getCountOfIntersectionsBetweenBookingAndSeatRoles (seats, booking));
		}

		private static int getCountOfIntersectionsBetweenBookingAndSeatRoles (IEnumerable<ISeat> seats, ISeatBooking seatBooking)
		{
			return (from seat in seats
					let personApplicationRoles = seatBooking.Person.PermissionInformation.ApplicationRoleCollection
					let intersectionCount = seat.Roles.Intersect (personApplicationRoles).Count()
					where seat.Roles.Count == intersectionCount
					select intersectionCount).Sum();
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