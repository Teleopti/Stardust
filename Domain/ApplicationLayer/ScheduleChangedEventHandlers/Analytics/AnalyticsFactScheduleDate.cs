using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleDate : IAnalyticsFactScheduleDate
	{
		public int ScheduleDateId { get; set; }
		public int ScheduleStartDateLocalId { get; set; }
		public DateTime ActivityStartTime { get; set; }
		public int ActivityStartDateId { get; set; }
		public DateTime ActivityEndTime { get; set; }
		public int ActivityEndDateId { get; set; }
		public DateTime ShiftStartTime { get; set; }
		public int ShiftStartDateId { get; set; }
		public DateTime ShiftEndTime { get; set; }
		public int ShiftEndDateId { get; set; }
		public int IntervalId { get; set; }
		public int ShiftStartIntervalId { get; set; }
		public int ShiftEndIntervalId { get; set; }
		public DateTime DatasourceUpdateDate { get; set; }
	}
}