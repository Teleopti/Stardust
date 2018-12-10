using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.SeatPlanning
{
	public class SeatPlanner : ISeatPlanner
	{
		private readonly ITeamRepository _teamRepository;
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly ISeatFrequencyCalculator _seatFrequencyCalculator;
		private readonly IPersonRepository _personRepository;
		private readonly ISeatBookingRequestAssembler _seatBookingRequestAssembler;
		private readonly ISeatPlanPersister _seatPlanPersister;

		public SeatPlanner(IPersonRepository personRepository, ISeatBookingRequestAssembler seatBookingRequestAssembler, ISeatPlanPersister seatPlanPersister, ITeamRepository teamRepository, ISeatMapLocationRepository seatMapLocationRepository, ISeatFrequencyCalculator seatFrequencyCalculator)
		{
			_personRepository = personRepository;
			_seatBookingRequestAssembler = seatBookingRequestAssembler;
			_seatPlanPersister = seatPlanPersister;
			_teamRepository = teamRepository;
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatFrequencyCalculator = seatFrequencyCalculator;
		}

		public ISeatPlanningResult Plan(IList<Guid> locationIds, IList<Guid> teamIds, DateOnlyPeriod dateOnlyPeriod, List<Guid> seatIds, List<Guid> personIds)
		{
			return seatIds != null
					? bookSeatsByManuallyChosenSeats(dateOnlyPeriod, locationIds, seatIds, personIds)
					: bookSeatsByLocationAndTeam(dateOnlyPeriod, locationIds, teamIds);
		}

		private ISeatPlanningResult bookSeatsByManuallyChosenSeats(DateOnlyPeriod dateOnlyPeriod, IList<Guid> locationIds, List<Guid> seatIds, List<Guid> personIds)
		{

			if (locationIds.Count != 1)
			{
				throw new ArgumentException("There should only be one location when manually planning by seats");
			}

			var seats = getSeatsInCommand(locationIds, seatIds);
			return createSeatPlansForPeriod(seats, personIds, dateOnlyPeriod);
		}

		private IEnumerable<ISeat> getSeatsInCommand (IList<Guid> locationIds, ICollection<Guid> seatIds)
		{
			var location = _seatMapLocationRepository.FindLocations(locationIds).Single();
			return location.Seats.Where(seat => seatIds.Contains(seat.Id.Value));
		}

		private ISeatPlanningResult bookSeatsByLocationAndTeam(DateOnlyPeriod dateOnlyPeriod, IEnumerable<Guid> locationIds, IEnumerable<Guid> teamIds)
		{
			var rootLocation = _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;
			if (rootLocation != null)
			{
				setIncludeInSeatPlan(rootLocation, locationIds);
			}

			var teams = _teamRepository.FindTeams(teamIds);
			return createSeatPlansForPeriod(rootLocation, teams, dateOnlyPeriod);
		}
		
		private static void setIncludeInSeatPlan(SeatMapLocation location, IEnumerable<Guid> locationsSelected)
		{
			location.IncludeInSeatPlan = locationsSelected.Any(l => l.Equals(location.Id));
			foreach (var childLocation in location.ChildLocations)
			{
				setIncludeInSeatPlan(childLocation, locationsSelected);
				}
		}

		private ISeatPlanningResult createSeatPlansForPeriod(SeatMapLocation rootSeatMapLocation, IEnumerable<ITeam> teams, DateOnlyPeriod period)
		{
			var people = getPeople(teams, period);
			var seatBookingInformation = _seatBookingRequestAssembler.CreateSeatBookingRequests(people, period);
			var seatFrequency = _seatFrequencyCalculator.GetSeatPopulationFrequency (period, people);
			
			SeatBookingForSeatLoader.AttachExistingSeatBookingsToSeats(rootSeatMapLocation, seatBookingInformation.ExistingSeatBookings);
			SeatAllocationManager.AllocateSeats(rootSeatMapLocation, seatBookingInformation, seatFrequency);

			_seatPlanPersister.Persist(period, seatBookingInformation);

			return getSeatPlanningResult (seatBookingInformation);

		}

		private ISeatPlanningResult createSeatPlansForPeriod(IEnumerable<ISeat> seats, IEnumerable<Guid> personIds, DateOnlyPeriod period)
		{
			var people = _personRepository.FindPeople(personIds).ToList();
			var seatBookingInformation = _seatBookingRequestAssembler.CreateSeatBookingRequests(people, period);
			var seatFrequency = _seatFrequencyCalculator.GetSeatPopulationFrequency(period, people);

			SeatBookingForSeatLoader.AttachExistingSeatBookingsToSeats(seats, seatBookingInformation.ExistingSeatBookings);
			SeatAllocationManager.AllocateSeats(seats, seatBookingInformation, seatFrequency);

			_seatPlanPersister.Persist(period, seatBookingInformation);

			return getSeatPlanningResult(seatBookingInformation);

		}

		private static ISeatPlanningResult getSeatPlanningResult(ISeatBookingRequestParameters seatBookingInformation)
		{

			var seatBookingRequests = seatBookingInformation.TeamGroupedBookings.Select (booking => booking.SeatBooking);

			return new SeatPlanningResult()
			{
				NumberOfBookingRequests = seatBookingRequests.Count(),
				RequestsGranted = seatBookingRequests.Count(booking => booking.Seat != null),
				NumberOfUnscheduledAgentDays = seatBookingInformation.NumberOfUnscheduledAgentDays
			};
		}

		private List<IPerson> getPeople(IEnumerable<ITeam> teams, DateOnlyPeriod period)
		{
			var people = new List<IPerson>();
			foreach (var team in teams)								 
			{
				//Maybe, if required in future due to performance use personscheduledayreadmodel 
				people.AddRange(_personRepository.FindPeopleBelongTeamWithSchedulePeriod(team, period));
			}

			return people;
		}

	}

}
