using System;

namespace Teleopti.Interfaces.Domain
{
	public class TrackedCommandInfo
	{
		public Guid OperatedPersonId { get; set; }
		public Guid TrackId { get; set; }
	}
}