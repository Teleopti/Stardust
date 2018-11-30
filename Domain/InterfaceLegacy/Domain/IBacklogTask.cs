using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public enum PlannedTimeTypeEnum
	{
		Scheduled,
		Calculated,
		Manual,
		Closed
	}

	public interface IBacklogTask
	{
		DateOnlyPeriod SpanningPeriod { get; }
		double TotalWorkItems { get; }
		TimeSpan TotalWorkTime { get; }
		TimeSpan AverageWorkTimePerItem { get; }
		TimeSpan GetTimeOnDate(DateOnly date);
		TimeSpan GetRealPlannedTimeOnDate(DateOnly date);
		void SetRealPlannedTimeOnDate(DateOnly date, TimeSpan time);
		TimeSpan GetScheduledTimeOnDate(DateOnly date);
		PlannedTimeTypeEnum PlannedTimeTypeOnDate(DateOnly date);
		void RecalculateDistribution();
		void Close(DateOnly date);
		void SetTimeOnDate(DateOnly date, TimeSpan timeSpan, PlannedTimeTypeEnum timeType);
		TimeSpan GetEstimatedIncomingBacklogOnDate(DateOnly date);
		void Open(DateOnly date);
		TimeSpan GetOverstaffTimeOnDate(DateOnly date);
		TimeSpan GetTimeOutsideSLA();
		TimeSpan GetBacklogOnDate(DateOnly date);
		TimeSpan GetRealScheduledTimeOnDate(DateOnly date);
		void SetActualBacklogOnDate(DateOnly date, TimeSpan backlog);
		TimeSpan? GetActualBacklogOnDate(DateOnly date);
		void SetRealScheduledTimeOnDate(DateOnly date, TimeSpan time);
		bool GetManualPlannedInfoOnDate(DateOnly date);
		void SetManualPlannedInfoOnDate(DateOnly date, bool isManualPlanneds);
		DateOnlyPeriod GetActivePeriod();
	}
}
