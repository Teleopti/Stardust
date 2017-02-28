using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class ModifyPersonAbsenceCommand : ITrackableCommand
	{
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public Guid PersonAbsenceId { get; set; }
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}
}