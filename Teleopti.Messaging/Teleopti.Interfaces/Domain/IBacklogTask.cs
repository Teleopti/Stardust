﻿using System;

namespace Teleopti.Interfaces.Domain
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
        void SetIncomingOverflowedWork(double workItems);
        TimeSpan GetTimeOnDate(DateOnly date);
        TimeSpan GetPlannedTimeOnDate(DateOnly date);
	    TimeSpan GetRealPlannedTimeOnDate(DateOnly date);
	    void SetRealPlannedTimeOnDate(DateOnly date, TimeSpan time);
        TimeSpan GetScheduledTimeOnDate(DateOnly date);
        PlannedTimeTypeEnum PlannedTimeTypeOnDate(DateOnly date);
        void RecalculateDistribution();
        void Close(DateOnly date);
        void SetTimeOnDate(DateOnly date, TimeSpan timeSpan, PlannedTimeTypeEnum timeType);
        TimeSpan GetEstimatedOutgoingBacklogOnDate(DateOnly date);
        TimeSpan GetEstimatedIncomingBacklogOnDate(DateOnly date);
        void ClearTimeOnDate(DateOnly date);
        void Open(DateOnly date);
        TimeSpan GetOverstaffTimeOnDate(DateOnly date);
        TimeSpan GetTimeOutsideSLA();
	    TimeSpan GetBacklogOnDate(DateOnly date);
	    TimeSpan GetRealScheduledTimeOnDate(DateOnly date);
	    void SetRealScheduledTimeOnDate(DateOnly date, TimeSpan time);
		 bool GetManualPlannedInfoOnDate(DateOnly date);
		 void SetManualPlannedInfoOnDate(DateOnly date, bool isManualPlanneds);
    }
}
