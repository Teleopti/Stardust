using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class TrackedCommandInfo
	{
		public Guid OperatedPersonId { get; set; }
		public Guid TrackId { get; set; }
	}
}