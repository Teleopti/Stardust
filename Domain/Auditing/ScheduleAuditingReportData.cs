using System;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class ScheduleAuditingReportData
	{
		public DateTime ModifiedAt { get; set; }
		public string ModifiedBy { get; set; }
		public string ScheduledAgent { get; set; }
		public string ShiftType { get; set; }
		public string AuditType { get; set; }
		public string Detail { get; set; }
		public DateTime ScheduleStart { get; set; }
		public DateTime ScheduleEnd { get; set; }
	}
}