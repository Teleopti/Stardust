using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddSeatPlanCommandHandler : IHandleCommand<AddSeatPlanCommand>
	{

		private readonly ISeatPlanner _seatPlanner;

		public AddSeatPlanCommandHandler(ISeatPlanner seatPlanner)
		{
			_seatPlanner = seatPlanner;
		}

		public void Handle(AddSeatPlanCommand command)
		{
			_seatPlanner.Plan(command.Locations, command.Teams, new DateOnlyPeriod(new DateOnly(command.StartDate), new DateOnly(command.EndDate)), command.SeatIds, command.PersonIds);

		}
	
	}
}


