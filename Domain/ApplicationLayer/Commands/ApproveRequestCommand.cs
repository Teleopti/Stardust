using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveRequestCommand : ITrackableCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public Guid PersonRequestId { get; set; }
		public Guid? AffectedRequestId { get; set; }
	}
}
