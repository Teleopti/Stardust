using System;
using System.Collections.ObjectModel;

namespace Teleopti.Analytics.Etl.Common.Interfaces.Common
{
    public interface IEtlJobSchedule
    {
        int ScheduleId { get; }
        DateTime Time { get; }
        DateTime LastTimeStarted { get; }
        DateTime TimeToRunNextJob { get; }
        string ScheduleName { get; }
        bool Enabled { get; }
        int OccursOnceAt { get; }
        int OccursEveryMinute { get; }
        int OccursEveryMinuteStartingAt { get; }
        int OccursEveryMinuteEndingAt { get; }
        DateTime PeriodicStartingTodayAt { get; }
        DateTime PeriodicEndingTodayAt { get; }
        string JobName { get; }
        ReadOnlyCollection<IEtlJobRelativePeriod> RelativePeriodCollection { get; }
        int DataSourceId { get; }
        string Description { get;  }
        JobScheduleType ScheduleType { get; }
		DateTime InsertDate { get; }
		string TenantName { get; set; }
		void SetScheduleIdOnPersistedItem(int scheduleId);
    }
}