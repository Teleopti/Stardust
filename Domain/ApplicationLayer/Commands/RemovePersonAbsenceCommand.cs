using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommand : ITrackableCommand
	{
		public IEnumerable<Guid> PersonAbsenceIds { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}