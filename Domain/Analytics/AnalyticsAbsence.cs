using System;

namespace Teleopti.Ccc.Domain.Analytics
{
	public class AnalyticsAbsence
	{
		public int AbsenceId { get; set; }
		public Guid AbsenceCode { get; set; }
		public bool InPaidTime { get; set; }
	}
}