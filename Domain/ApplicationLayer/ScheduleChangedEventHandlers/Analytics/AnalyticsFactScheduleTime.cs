using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure.Analytics;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	public class AnalyticsFactScheduleTime : IAnalyticsFactScheduleTime
	{
		public AnalyticsFactScheduleTime()
		{
			OverTimeId = -1;
			AbsenceId = -1;
			ActivityId = -1;
			ShiftCategoryId = -1;
		}

		public int ShiftCategoryId { get; set; }
		public int ScenarioId { get; set; }
		public int ActivityId { get; set; }
		public int AbsenceId { get; set; }
		public int OverTimeId { get; set; }
		public int ScheduledMinutes { get; set; }
		public int ScheduledAbsenceMinutes { get; set; }
		public int ScheduledActivityMinutes { get; set; }
		public int ContractTimeMinutes { get; set; }
		public int ContractTimeActivityMinutes { get; set; }
		public int ContractTimeAbsenceMinutes { get; set; }
		public int WorkTimeMinutes { get; set; }
		public int WorkTimeActivityMinutes { get; set; }
		public int WorkTimeAbsenceMinutes { get; set; }
		public int OverTimeMinutes { get; set; }
		public int ReadyTimeMinutes { get; set; }
		public int PaidTimeMinutes { get; set; }
		public int PaidTimeActivityMinutes { get; set; }
		public int PaidTimeAbsenceMinutes { get; set; }
		public int ShiftLengthId { get; set; }
		public int PlannedOvertimeMinutes { get; set; }
	}
}