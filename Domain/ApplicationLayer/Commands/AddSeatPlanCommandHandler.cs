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
		
		private readonly ICurrentScenario _scenario;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly ITeamRepository _teamRepository;
		private readonly IPersonRepository _personRepository;
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly ISeatBookingRepository _seatBookingRepository;
		private readonly ISeatPlanRepository _seatPlanRepository;

		public AddSeatPlanCommandHandler(	IScheduleRepository scheduleRepository, ITeamRepository teamRepository, 
											IPersonRepository personRepository, ICurrentScenario scenario, 
											ISeatMapLocationRepository seatMapLocationRepository, ISeatBookingRepository seatBookingRepository, 
											ISeatPlanRepository seatPlanRepository)
		{
			_scenario = scenario;
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatBookingRepository = seatBookingRepository;
			_seatPlanRepository = seatPlanRepository;
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
			var seatPlanner = new SeatPlanner(_scenario.Current(), _personRepository, _scheduleRepository, _seatBookingRepository,_seatPlanRepository);

			seatPlanner.CreateSeatPlansForPeriod (rootLocation, teams, period, command.TrackedCommandInfo);

		}
		
		private static void setIncludeInSeatPlan(SeatMapLocation location, IEnumerable<Guid> locationsSelected)
		{
			location.IncludeInSeatPlan = locationsSelected.Any(l => l.Equals(location.Id));
			foreach (var childLocation in location.ChildLocations)
			{
				setIncludeInSeatPlan (childLocation, locationsSelected);
			}
		}
	}
}


