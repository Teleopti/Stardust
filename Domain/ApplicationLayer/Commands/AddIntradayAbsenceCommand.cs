using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public class AddIntradayAbsenceCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartDateTime { get; set; }
		public DateTime EndDateTime { get; set; }
	}
}