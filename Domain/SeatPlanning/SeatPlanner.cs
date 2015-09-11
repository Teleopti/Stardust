using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanner : ISeatPlanner
	{
		private readonly SeatBookingRequestAssembler _seatBookingRequestAssembler;
		private readonly SeatPlanPersister _seatPlanPersister;

		public SeatPlanner(IScenario currentScenario,
							IPersonRepository personRepository,
							IScheduleRepository scheduleRepository,
							ISeatBookingRepository seatBookingRepository, 
							ISeatPlanRepository seatPlanRepository)
		{
			_seatBookingRequestAssembler = new SeatBookingRequestAssembler(personRepository, scheduleRepository, seatBookingRepository, currentScenario);
			_seatPlanPersister = new SeatPlanPersister(seatBookingRepository, seatPlanRepository);
		}

		public void CreateSeatPlansForPeriod(SeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period, TrackedCommandInfo trackedCommandInfo)
		{
			var seatBookingInformation = _seatBookingRequestAssembler.AssembleAndGroupSeatBookingRequests(rootSeatMapLocation, teams, period);
			allocateSeatsToRequests(rootSeatMapLocation, seatBookingInformation);
			_seatPlanPersister.Persist(period, seatBookingInformation);
		}
		
		private static void allocateSeatsToRequests (SeatMapLocation rootSeatMapLocation, ISeatBookingRequestParameters seatBookingInformation)
		{
			var groupedRequests = groupByDateAndTeam (seatBookingInformation.TeamGroupedBookings);
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
