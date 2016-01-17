using System;
using System.Linq;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.Service
{
	internal class SchedulePriority
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SchedulePriority));

		public IEtlJobSchedule GetTopPriority(IEtlJobScheduleCollection jobScheduleCollection, DateTime now, DateTime serviceStartTime)
		{

			var scheulesOverDue = from s in jobScheduleCollection
								  where s.Enabled
								  where s.TimeToRunNextJob > s.LastTimeStarted
								  where s.TimeToRunNextJob < now
								  where s.TimeToRunNextJob > serviceStartTime
								  where (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt) || s.ScheduleType == JobScheduleType.OccursDaily
								  orderby s.TimeToRunNextJob
								  select s;

			foreach (var etlSchedule in jobScheduleCollection)
			{
				Console.WriteLine("Schedule: Next: {0} / Last: {1}", etlSchedule.TimeToRunNextJob, etlSchedule.LastTimeStarted);
			}

			if (scheulesOverDue.Count() >= 1)
			{
				Console.WriteLine("Overdue: Next: {0} / Last: {1}", scheulesOverDue.First().TimeToRunNextJob, scheulesOverDue.First().LastTimeStarted);
			}

			if (scheulesOverDue.Count() >= 2)
			{
				var logText = "There are " + scheulesOverDue.Count() + "enqueued jobs";
				log.Info(logText);
			}

			return scheulesOverDue.Count() == 0 ? null : scheulesOverDue.First();
		}

	}
}