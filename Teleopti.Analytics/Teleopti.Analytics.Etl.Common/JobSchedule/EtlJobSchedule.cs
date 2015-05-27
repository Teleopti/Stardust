using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.JobSchedule
{
    public class EtlJobSchedule : IEtlJobSchedule
    {
        private readonly DateTime _occursDailyAt;
        private readonly int _cyclicInterval;
        private readonly DateTime _endTime;
        private readonly DateTime _startTime;
        private readonly IList<IEtlJobRelativePeriod> _relativePeriodCollection;

        public EtlJobSchedule(int scheduleId, String scheduleName, bool enabled, int occursDailyAt,
                            string jobName, int relativePeriodStart, int relativePeriodEnd, int dataSourceId,
                            string description, IEtlJobLogCollection etlJobLogCollection, 
                            IList<IEtlJobRelativePeriod> relativePeriodCollection)
        {
            _etlJobLogCollection = etlJobLogCollection;

            var dateTime = DateTime.Today;

            _occursDailyAt = dateTime.Add(new TimeSpan(0, occursDailyAt, 0));

            ScheduleId = scheduleId;
            ScheduleType = JobScheduleType.OccursDaily;
            ScheduleName = scheduleName;
            Enabled = enabled;
            OccursOnceAt = occursDailyAt;
            JobName = jobName;
            RelativePeriodStart = relativePeriodStart;
            RelativePeriodEnd = relativePeriodEnd;
            DataSourceId = dataSourceId;
            Description = description;
            _relativePeriodCollection = relativePeriodCollection;
        }

        public EtlJobSchedule(int scheduleId, String scheduleName, bool enabled, int cyclicInterval, int startTime, int endTime,
                            string jobName, int relativePeriodStart, int relativePeriodEnd, int dataSourceId,
                            string description, IEtlJobLogCollection etlJobLogCollection, DateTime serverStartTime, 
                            IList<IEtlJobRelativePeriod> relativePeriodCollection)
        {
            _etlJobLogCollection = etlJobLogCollection;

            _cyclicInterval = cyclicInterval;

            var dateTime = DateTime.Today;

            _startTime = dateTime.Add(new TimeSpan(0, startTime, 0));
            Trace.WriteLine(_startTime.ToString());

            _endTime = dateTime.Add(new TimeSpan(0, endTime, 0));
            Trace.WriteLine(_endTime.ToString());

            ScheduleId = scheduleId;
            ScheduleType = JobScheduleType.Periodic;
            ScheduleName = scheduleName;
            Enabled = enabled;
            OccursEveryMinute = cyclicInterval;
            OccursEveryMinuteStartingAt = startTime;
            OccursEveryMinuteEndingAt = endTime;
            JobName = jobName;
            RelativePeriodStart = relativePeriodStart;
            RelativePeriodEnd = relativePeriodEnd;
            DataSourceId = dataSourceId;
            Description = description;
            _serverStartTime = serverStartTime;
            _relativePeriodCollection = relativePeriodCollection;
        }

        #region IScheduleOccursDaily Members

        IEtlJobLogCollection _etlJobLogCollection;


        private DateTime _serverStartTime;

        public IEtlJobLogCollection RecentEtlJobLogData
        {
            get
            {

                return _etlJobLogCollection;
            }
        }

        public DateTime Time
        {
            get { return _occursDailyAt; }
        }

        public DateTime LastTimeStarted
        {
            get
            {
                var result = from l in RecentEtlJobLogData
                             where ScheduleId == l.ScheduleId
                             orderby l.StartTime
                             select l;

                if ((result.Count() == 0) || (_serverStartTime > result.Last().StartTime))
                {
                    return _serverStartTime;
                }

                return result.Last().StartTime;
            }
        }

        public DateTime TimeToRunNextJob
        {
            get
            {
                DateTime dt;
                if (ScheduleType == JobScheduleType.OccursDaily)
                {
                    dt = _occursDailyAt;
                }
                else //Periodic
                {
                    dt = _startTime;
                    while (dt <= LastTimeStarted)
                    {
                        dt = dt.AddMinutes(_cyclicInterval);
                    }
                    if (dt > _endTime)
                    {
                        dt = _startTime.AddDays(1);
                    }
                }

                return dt;
            }
        }

        #endregion

        public int ScheduleId { get; private set; }
        public JobScheduleType ScheduleType { get; private set; }
        public void SetScheduleIdOnPersistedItem(int scheduleId)
        {
            // Only allow this if new item, when this.ScheuleId = -1
            if (ScheduleId == -1)
            {
                ScheduleId = scheduleId;
            }
        }

        public string ScheduleName { get; private set; }
        public bool Enabled { get; private set; }
        public int OccursOnceAt { get; private set; }
        public int OccursEveryMinute { get; private set; }
        public int OccursEveryMinuteStartingAt { get; private set; }
        public int OccursEveryMinuteEndingAt { get; private set; }
        
        public DateTime PeriodicStartingTodayAt
        {
            get
            {
                var tonight = DateTime.SpecifyKind(DateTime.Today,DateTimeKind.Local);
                return tonight.AddMinutes(OccursEveryMinuteStartingAt);
            }
        }

        public DateTime PeriodicEndingTodayAt
        {
            get
            {
                var tonight = DateTime.SpecifyKind(DateTime.Today, DateTimeKind.Local);
                return tonight.AddMinutes(OccursEveryMinuteEndingAt);
            }
        }

        public string JobName { get; private set; }
        public int RelativePeriodStart { get; private set; }
        public int RelativePeriodEnd { get; private set; }
        public ReadOnlyCollection<IEtlJobRelativePeriod> RelativePeriodCollection
        {
            get { return new ReadOnlyCollection<IEtlJobRelativePeriod>(_relativePeriodCollection); }
        }

        public int DataSourceId { get; private set; }
        public string Description { get; private set; }
    }
}