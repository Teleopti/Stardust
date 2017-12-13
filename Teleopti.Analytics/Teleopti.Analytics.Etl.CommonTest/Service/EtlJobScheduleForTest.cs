using System;
using System.Collections.ObjectModel;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.CommonTest.Service
{
	public class EtlJobScheduleForTest : IEtlJobSchedule
	{
		
		public int ScheduleId { get; set; }
		public DateTime Time { get; set; }
		public DateTime LastTimeStarted { get; set; }
		public DateTime TimeToRunNextJob { get; set; }
		public string ScheduleName { get; set; }
		public bool Enabled { get; set; }
		public int OccursOnceAt { get; set; }
		public int OccursEveryMinute { get; set; }
		public int OccursEveryMinuteStartingAt { get; set; }
		public int OccursEveryMinuteEndingAt { get; set; }
		public DateTime PeriodicStartingTodayAt { get; set; }
		public DateTime PeriodicEndingTodayAt { get; set; }
		public string JobName { get; set; }
		public int RelativePeriodStart { get; set; }
		public int RelativePeriodEnd { get; set; }
		public ReadOnlyCollection<IEtlJobRelativePeriod> RelativePeriodCollection { get; set; }
		public int DataSourceId { get; set; }
		public string Description { get; set; }
		public JobScheduleType ScheduleType { get; set; }
		public void SetScheduleIdOnPersistedItem(int scheduleId)
		{
			throw new NotImplementedException();
		}
	}
}