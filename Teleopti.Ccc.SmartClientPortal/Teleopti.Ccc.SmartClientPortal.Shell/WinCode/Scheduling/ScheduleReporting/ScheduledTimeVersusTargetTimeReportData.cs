using System;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleReporting
{
	public class ScheduledTimeVersusTargetTimeReportData : IScheduledTimeVersusTargetTimeReportData
	{
		public string PersonName { get; set; }
		public DateTime PeriodFrom { get; set; }
		public DateTime PeriodTo { get; set; }
		public Double TargetTime { get; set; }
		public int TargetDayOffs { get; set; }
		public Double ScheduledTime { get; set; }
		public int ScheduledDayOffs { get; set; }
		public Double DifferenceTime { get { return ScheduledTime - TargetTime; } }
		public int DifferenceDayOffs { get { return ScheduledDayOffs - TargetDayOffs; } }
	}
}
