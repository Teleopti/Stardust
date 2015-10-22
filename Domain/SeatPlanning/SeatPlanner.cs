using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanner : ISeatPlanner
	{
		private readonly IPersonRepository _personRepository;
		private readonly ISeatBookingRequestAssembler _seatBookingRequestAssembler;
		private readonly ISeatPlanPersister _seatPlanPersister;

		public SeatPlanner(IPersonRepository personRepository, ISeatBookingRequestAssembler seatBookingRequestAssembler, ISeatPlanPersister seatPlanPersister)
		{
			_personRepository = personRepository;
			_seatBookingRequestAssembler = seatBookingRequestAssembler;
			_seatPlanPersister = seatPlanPersister;
		}

		public void CreateSeatPlansForPeriod(ISeatMapLocation rootSeatMapLocation, ICollection<ITeam> teams, DateOnlyPeriod period)
		{
			var people = getPeople(teams, period);
			var seatBookingInformation = _seatBookingRequestAssembler.AssembleAndGroupSeatBookingRequests(people, period);
			SeatPlannerHelper.AttachExistingSeatBookingsToSeats(rootSeatMapLocation, seatBookingInformation.ExistingSeatBookings);

			allocateSeatsToRequests(rootSeatMapLocation, seatBookingInformation);
			_seatPlanPersister.Persist(period, seatBookingInformation);
		}

		public void CreateSeatPlansForPeriod(IEnumerable<ISeat> seats, List<Guid> personIds, DateOnlyPeriod period)
		{
			var people = _personRepository.FindPeople(personIds).ToList();
			var seatBookingInformation = _seatBookingRequestAssembler.AssembleAndGroupSeatBookingRequests(people, period);
			SeatPlannerHelper.AttachExistingSeatBookingsToSeats(seats, seatBookingInformation.ExistingSeatBookings);

			allocateSeatsToRequests(seats, seatBookingInformation);
			_seatPlanPersister.Persist(period, seatBookingInformation);
		}

		private List<IPerson> getPeople(IEnumerable<ITeam> teams, DateOnlyPeriod period)
		{
			var people = new List<IPerson>();
			foreach (var team in teams)
			{
				//RobTodo: review: Try to use personscheduledayreadmodel to improve performance
				people.AddRange(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(team, period));
			}

			return people;
		}

		private void allocateSeatsToRequests(IEnumerable<ISeat> seats, ISeatBookingRequestParameters seatBookingInformation)
		{
			var groupedRequests = groupByDateAndTeam(seatBookingInformation.TeamGroupedBookings);
			new SeatLevelAllocator(seats).AllocateSeats(groupedRequests);
		}

		private static void allocateSeatsToRequests(ISeatMapLocation rootSeatMapLocation, ISeatBookingRequestParameters seatBookingInformation)
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
