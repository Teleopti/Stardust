using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ApproveRequestCommand : ITrackableCommand, IErrorAttachedCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public IEnumerable<Guid> PersonRequestIds { get; set; }
		public IEnumerable<Guid> AffectedRequestIds { get; set; }
		public IList<string> ErrorMessages { get; set; }
	}
}
