using System;
using System.Linq;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.Service
{
    internal class SchedulePriority
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SchedulePriority));

        public IEtlJobSchedule GetTopPriority(IEtlJobScheduleCollection jobScheduleCollection, DateTime now, DateTime serviceStartTime)
        {

            IOrderedEnumerable<IEtlJobSchedule> scheulesOverDue = from s in jobScheduleCollection
                                  where s.Enabled
                                  where s.TimeToRunNextJob > s.LastTimeStarted
                                  where s.TimeToRunNextJob < now
                                  where s.TimeToRunNextJob > serviceStartTime
                                  where (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt) || s.ScheduleType == JobScheduleType.OccursDaily
                                  orderby s.TimeToRunNextJob
                                  select s;

            foreach (IEtlJobSchedule etlSchedule in jobScheduleCollection)
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
                Log.Info(logText);
            }

            return scheulesOverDue.Count() == 0 ? null : scheulesOverDue.First();
        }

    }
}