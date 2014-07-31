using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddFullDayAbsenceCommand : ITrackedCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}