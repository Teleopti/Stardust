using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommandHandler : IHandleCommand<AddSeatPlanCommand>
	{
		//private readonly IWriteSideRepository<ISeatPlan> _seatPlanRepository;
		private readonly ICurrentScenario _scenario;
		
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IPublicNoteRepository _publicNoteRepository;

		//RobTodo: When we need to store Seat Plans, we need to create a SeatPlanRepository, eg:
		// public class SeatPlanRepository : Repository<ISeatPlan>, ISeatPlanRepository, IWriteSideRepository<ISeatPlan>, IProxyForId<ISeatPlan>
		// then it can be added to the constructor ready for autofac injection
		//public AddSeatPlanCommandHandler(IWriteSideRepository<ISeatPlan>seatPlanRepository, ICurrentScenario scenario)
		public AddSeatPlanCommandHandler(IScheduleRepository scheduleRepository, ITeamRepository teamRepository, IPersonRepository personRepository, ICurrentScenario scenario, IPublicNoteRepository publicNoteRepository)
		{
			//_seatPlanRepository = seatPlanRepository;
			_scenario = scenario;
			_publicNoteRepository = publicNoteRepository;
			_scheduleRepository = scheduleRepository;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
		}

		public void Handle(AddSeatPlanCommand command)
		{

			//Robtodo: temporary code to get locations - just throw away code for the prototype 
			var rootLocation = getLocation (command.LocationsFromFile, command.Locations);
			
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
			var teams = _teamRepository.FindTeams (command.Teams);

			var seatPlan = new SeatPlan(_scenario.Current(), _publicNoteRepository, _personRepository, _scheduleRepository);
			seatPlan.CreateSeatPlan(rootLocation, teams,period, command.TrackedCommandInfo);

			//Robtodo: Persist Seat Plan record later..
			//_seatPlanRepository.Add(seatPlan);

		}
		

		private static Location getLocation(dynamic location, IList<Guid> locations)
		{
			if (location == null)
			{
				return null;
			}

			var locationGuid = Guid.Parse (location.id);

			var childLocations = getChildLocations(location, locations);
			var seats = getSeats(location);
			var isSelectedForSeatPlanning = locations.Any(l => l.Equals(locationGuid));

			var loc = new Location()
			{
				Id = locationGuid,
				Name = location.name,
				IncludeInSeatPlan = isSelectedForSeatPlanning
			};
			
			loc.AddChildren (childLocations);
			loc.AddSeats (seats);


			return loc;
		}

		private static IEnumerable<Location> getChildLocations(dynamic location, IList<Guid> locations)
		{
			if (location.childLocations != null)
			{
				var childLocations = new List<Location>();

				foreach (var child in location.childLocations)
				{
					childLocations.Add(getLocation(child, locations));
				}

				return childLocations;
			}

			return null;
		}

		private static IEnumerable<Seat> getSeats(dynamic location)
		{
			if (location.seats != null)
			{
				var seats = new List<Seat>();
				foreach (var seat in location.seats)
				{
					var agentSeat = new Seat(Guid.Parse(seat.id), seat.name);
					seats.Add(agentSeat);
				}
				return seats;
			}

			return null;
		}




		

	}
}


