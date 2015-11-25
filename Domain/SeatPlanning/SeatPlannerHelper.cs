using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public static class SeatPlannerHelper
	{
		public static void AttachExistingSeatBookingsToSeats(ISeatMapLocation rootSeatMapLocation, IList<ISeatBooking> existingSeatBookings)
		{
			var rootLocation = rootSeatMapLocation as SeatMapLocation;
			if (rootLocation != null)
			{
				AttachExistingSeatBookingsToSeats(rootSeatMapLocation.Seats, existingSeatBookings);
				rootLocation 
					.ChildLocations
					.ForEach(location => AttachExistingSeatBookingsToSeats(location, existingSeatBookings));
			}
		}

		public static void AttachExistingSeatBookingsToSeats(IEnumerable<ISeat> seats, IList<ISeatBooking> existingSeatBookings)
		{
			foreach (var seat in seats)
			{
				var seatBookings = existingSeatBookings.Where(booking => Equals(booking.Seat, seat)).ToList();
				seat.AddSeatBookings(seatBookings);
			}
		}
		
		public static void AllocateSeatsToRequests(IEnumerable<ISeat> seats, ISeatBookingRequestParameters seatBookingInformation)
		{
			var groupedRequests = groupByDateAndTeam(seatBookingInformation.TeamGroupedBookings);
			new SeatLevelAllocator(seats).AllocateSeats(groupedRequests);
		}

		public static void AllocateSeatsToRequests(SeatMapLocation rootSeatMapLocation, ISeatBookingRequestParameters seatBookingInformation)
		{
			var groupedRequests = groupByDateAndTeam(seatBookingInformation.TeamGroupedBookings);
			new SeatAllocator(rootSeatMapLocation).AllocateSeats(groupedRequests);
		}

		private static SeatBookingRequest[] groupByDateAndTeam(IEnumerable<ITeamGroupedBooking> bookingsByTeam)
		{
			var seatBookingsByDateAndTeam = bookingsByTeam
				.GroupBy(booking => booking.SeatBooking.BelongsToDate)
				.Select(x => new
				{
					Category = x.Key,
					TeamGroups = x.ToList()
						.GroupBy(y => y.Team)
				});

			var seatBookingRequests =
				from day in seatBookingsByDateAndTeam
				from teamGroups in day.TeamGroups
				select teamGroups.Select(team => team.SeatBooking)
					into teamBookingsforDay
					select new SeatBookingRequest(teamBookingsforDay.ToArray());

			return seatBookingRequests.ToArray();
		}
	}
}