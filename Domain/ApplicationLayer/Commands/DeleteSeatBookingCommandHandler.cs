using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DeleteSeatBookingCommandHandler : IHandleCommand<DeleteSeatBookingCommand>
	{
		private readonly ISeatPlanPersister _seatPlanPerister;

		public DeleteSeatBookingCommandHandler(ISeatPlanPersister seatPlanPerister)
		{
			_seatPlanPerister = seatPlanPerister;
		}

		public void Handle (DeleteSeatBookingCommand command)
		{

			_seatPlanPerister.RemoveSeatBooking (command.SeatBookingId);


		}
	}
}