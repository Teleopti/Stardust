using System;

namespace Teleopti.Interfaces.Infrastructure.Analytics
{
	public interface IAnalyticsFactScheduleDate
	{
		int ScheduleDateId { get; set; }
		int ScheduleStartDateLocalId { get; set; }
		DateTime ActivityStartTime { get; set; }
		int ActivityStartDateId { get; set; }
		DateTime ActivityEndTime { get; set; }
		int ActivityEndDateId { get; set; }
		DateTime ShiftStartTime { get; set; }
		int ShiftStartDateId { get; set; }
		DateTime ShiftEndTime { get; set; }
		int ShiftEndDateId { get; set; }
		int IntervalId { get; set; }
		int ShiftStartIntervalId { get; set; }
		int ShiftEndIntervalId { get; set; }
		DateTime DatasourceUpdateDate { get; set; }
	}
}