using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommandHandler : IHandleCommand<AddSeatPlanCommand>
	{
		
		
		private readonly ITeamRepository _teamRepository;
		private readonly ISeatMapLocationRepository _seatMapLocationRepository;
		private readonly ISeatPlanner _seatPlanner;

		public AddSeatPlanCommandHandler(ITeamRepository teamRepository, ISeatMapLocationRepository seatMapLocationRepository, ISeatPlanner seatPlanner)
		{
			_seatMapLocationRepository = seatMapLocationRepository;
			_seatPlanner = seatPlanner;
			_teamRepository = teamRepository;
		}

		public void Handle(AddSeatPlanCommand command)
		{

			if (command.SeatIds != null)
			{
				handleSeatsInSeatPlanCommand(command);
			}
			else if (command.Locations != null)
			{
				handleLocationsInSeatPlanCommand(command);
			}
			
		}

		private void handleLocationsInSeatPlanCommand(AddSeatPlanCommand command)
		{
			
			var rootLocation = _seatMapLocationRepository.LoadRootSeatMap();
			if (rootLocation != null)
			{
				setIncludeInSeatPlan(rootLocation, command.Locations);
			}

			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
			var teams = _teamRepository.FindTeams(command.Teams);

			_seatPlanner.CreateSeatPlansForPeriod(rootLocation, teams, period);

		}

		private void handleSeatsInSeatPlanCommand(AddSeatPlanCommand command)
		{

			if ( command.Locations.Count != 1)
			{
				throw new ArgumentException("There should only be one location when planning by seats");
			}

			var seats = getSeatsInCommand(command);
			var period = new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate));
			_seatPlanner.CreateSeatPlansForPeriod(seats, command.PersonIds, period);

		}


		private IEnumerable<ISeat> getSeatsInCommand(AddSeatPlanCommand command)
		{
			var location = _seatMapLocationRepository.FindLocations(command.Locations).Single();
			return location.Seats.Where(seat => command.SeatIds.Contains(seat.Id.Value));
		}

		private static void setIncludeInSeatPlan(ISeatMapLocation location, IEnumerable<Guid> locationsSelected)
		{
			var seatMapLocation = location as SeatMapLocation;
			if (seatMapLocation != null)
			{
				seatMapLocation.IncludeInSeatPlan = locationsSelected.Any(l => l.Equals(location.Id));
				foreach (var childLocation in seatMapLocation.ChildLocations)
				{
					setIncludeInSeatPlan(childLocation, locationsSelected);
				}	
			}

			
		}
	}
}


