using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
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
		private readonly IPublicNoteRepository _publicNoteRepository;
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly IWriteSideRepository<ISeatBooking> _seatBookingRepository;

		public AddSeatPlanCommandHandler(IScheduleRepository scheduleRepository, ITeamRepository teamRepository, IPersonRepository personRepository, ICurrentScenario scenario, IPublicNoteRepository publicNoteRepository, ISeatMapLocationRepository seatMapLocationRepository, IWriteSideRepository<ISeatBooking> seatBookingRepository)
		{
			_scenario = scenario;
			_publicNoteRepository = publicNoteRepository;
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatBookingRepository = seatBookingRepository;
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


			//Robtodo: review this write side repository to ISeatBookingRepository
			var seatPlan = new SeatPlan(_scenario.Current(), _publicNoteRepository, _personRepository, _scheduleRepository, _seatBookingRepository as ISeatBookingRepository);

			seatPlan.CreateSeatPlan(rootLocation, teams, period, command.TrackedCommandInfo)
				.ForEach(booking => _seatBookingRepository.Add(booking));
		}
		
		private static void setIncludeInSeatPlan(SeatMapLocation location, IList<Guid> locationsSelected)
		{
			location.IncludeInSeatPlan = locationsSelected.Any(l => l.Equals(location.Id));
			foreach (var childLocation in location.ChildLocations)
			{
				setIncludeInSeatPlan (childLocation, locationsSelected);
			}
		}
	}
}


