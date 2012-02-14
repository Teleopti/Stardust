using System;
using System.Linq;
using log4net;
using Teleopti.Analytics.Etl.Interfaces.Common;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
    internal class SchedulePriority
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(SchedulePriority));

        public IEtlSchedule GetTopPriority(IEtlScheduleCollection scheduleCollection, DateTime now, DateTime serviceStartTime)
        {

            IOrderedEnumerable<IEtlSchedule> scheulesOverDue = from s in scheduleCollection
                                  where s.Enabled
                                  where s.TimeToRunNextJob > s.LastTimeStarted
                                  where s.TimeToRunNextJob < now
                                  where s.TimeToRunNextJob > serviceStartTime
                                  where (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt) || s.ScheduleType == JobScheduleType.OccursDaily
                                  orderby s.TimeToRunNextJob
                                  select s;

            foreach (IEtlSchedule etlSchedule in scheduleCollection)
            {
                Console.WriteLine("Check this out: ");
                Console.WriteLine("next time to run: " + etlSchedule.TimeToRunNextJob);
                Console.WriteLine("last time run: " + etlSchedule.LastTimeStarted);
            }

            if (scheulesOverDue.Count() >= 1)
            {
                Console.WriteLine("last: " + scheulesOverDue.First().LastTimeStarted);
                Console.WriteLine("next: " + scheulesOverDue.First().TimeToRunNextJob);
            }

            if (scheulesOverDue.Count() >= 2)
            {
                String logText = "There are " + scheulesOverDue.Count() + "enqueued jobs";
                _log.Info(logText);
            }

            return scheulesOverDue.Count() == 0 ? null : scheulesOverDue.First();
        }

    }
}