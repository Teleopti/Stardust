using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MyTeamRemovePersonAbsenceCommand : ITrackableCommand
	{
		public Guid PersonAbsenceId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}