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
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;

		//RobTodo: When we need to store Seat Plans, we need to create a SeatPlanRepository, eg:
		// public class SeatPlanRepository : Repository<ISeatPlan>, ISeatPlanRepository, IWriteSideRepository<ISeatPlan>, IProxyForId<ISeatPlan>
		// then it can be added to the constructor ready for autofac injection
		//public AddSeatPlanCommandHandler(IWriteSideRepository<ISeatPlan>seatPlanRepository, ICurrentScenario scenario)
		public AddSeatPlanCommandHandler(IScheduleRepository scheduleRepository, ITeamRepository teamRepository, IPersonRepository personRepository, ICurrentScenario scenario, IPublicNoteRepository publicNoteRepository, ISeatMapLocationRepository seatMapLocationRepository)
		{
			//_seatPlanRepository = seatPlanRepository;
			_scenario = scenario;
			_publicNoteRepository = publicNoteRepository;
			_seatMapLocationRepository = seatMapLocationRepository;
			_scheduleRepository = scheduleRepository;
			_teamRepository = teamRepository;
			_personRepository = personRepository;
		}

		public void Handle(AddSeatPlanCommand command)
		{

			var rootLocation = _seatMapLocationRepository.LoadRootSeatMap() as SeatMapLocation;
			if (rootLocation != null)
			{
				setIncludeInSeatPlan(rootLocation, command.Locations);
			}

			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
			var teams = _teamRepository.FindTeams (command.Teams);

			var seatPlan = new SeatPlan(_scenario.Current(), _publicNoteRepository, _personRepository, _scheduleRepository);
			seatPlan.CreateSeatPlan(rootLocation, teams,period, command.TrackedCommandInfo);

			//Robtodo: Persist Seat Plan record later..
			

		}
		
		private static void setIncludeInSeatPlan(SeatMapLocation rootLocation, IList<Guid> locationsSelected)
		{
			foreach (var location in rootLocation.ChildLocations)
			{
				location.IncludeInSeatPlan = locationsSelected.Any(l => l.Equals(location.Id));
				setIncludeInSeatPlan (location, locationsSelected);
			}
		}

	}
}


