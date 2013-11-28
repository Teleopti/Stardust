using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateOnly Date { get; set; }
		public TimeOfDay StartTime { get; set; }
		public TimeOfDay EndTime { get; set; }
	}
}