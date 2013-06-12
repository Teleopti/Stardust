using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class AddFullDayAbsenceCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}