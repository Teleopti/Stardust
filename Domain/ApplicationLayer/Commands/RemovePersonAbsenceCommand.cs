using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class RemovePersonAbsenceCommand : ITrackedCommand
	{
		public Guid PersonAbsenceId { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}