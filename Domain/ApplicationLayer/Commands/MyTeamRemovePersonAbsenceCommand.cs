using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class MyTeamRemovePersonAbsenceCommand : ITrackableCommand
	{
		public Guid PersonAbsenceId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
		public ActionErrorMessage Errors { get; set; }
	}
}