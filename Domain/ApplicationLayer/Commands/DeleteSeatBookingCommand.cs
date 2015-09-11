using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class DeleteSeatBookingCommand : ITrackableCommand
	{
		public Guid SeatBookingId { get; set; }

		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}