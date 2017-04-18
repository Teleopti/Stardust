using System;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleReporting
{
	public interface IScheduledTimeVersusTargetTimeReportData
	{
		string PersonName { get; set; }
		DateTime PeriodFrom { get; set; }
		DateTime PeriodTo { get; set; }
		Double TargetTime { get; set; }
		int TargetDayOffs { get; set; }
		Double ScheduledTime { get; set; }
		int ScheduledDayOffs { get; set; }
		Double DifferenceTime { get;} 
		int DifferenceDayOffs { get;}
	}
}
