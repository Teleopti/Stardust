using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatAllocationManager
	{
		public static void AllocateSeats(IEnumerable<ISeat> seats, ISeatBookingRequestParameters seatBookingInformation, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var groupedRequests = groupByDateAndTeam(seatBookingInformation.TeamGroupedBookings);
			new SeatLevelAllocator(seats, seatFrequencies).AllocateSeats(groupedRequests);
		}

		public static void AllocateSeats(SeatMapLocation rootSeatMapLocation, ISeatBookingRequestParameters seatBookingInformation, Dictionary<Guid, List<ISeatOccupancyFrequency>> seatFrequencies)
		{
			var groupedRequests = groupByDateAndTeam(seatBookingInformation.TeamGroupedBookings);
			new SeatAllocator(seatFrequencies, rootSeatMapLocation).AllocateSeats(groupedRequests);
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