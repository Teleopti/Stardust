using System;
using System.Linq;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class SchedulePriority
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SchedulePriority));

		public IEtlJobSchedule GetTopPriority(IEtlJobScheduleCollection jobScheduleCollection, DateTime now, DateTime serviceStartTime)
		{
			var scheduledJobsRunnable = jobScheduleCollection
				.Where(x => x.ScheduleType == JobScheduleType.Manual && x.Enabled)
				.OrderBy(x => x.InsertDate)
				.ToList();


			if (!scheduledJobsRunnable.Any())
			{
				var jobs = from s in jobScheduleCollection
					where s.Enabled
						  && s.TimeToRunNextJob > s.LastTimeStarted
						  && s.TimeToRunNextJob < now
						  && s.TimeToRunNextJob > serviceStartTime
						  && (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt || s.ScheduleType == JobScheduleType.OccursDaily)
					orderby s.TimeToRunNextJob
					select s;

				scheduledJobsRunnable = jobs.ToList();
			}

			foreach (var etlSchedule in jobScheduleCollection
				.Where(x => x.Enabled && x.ScheduleType == JobScheduleType.Manual)
				.OrderBy(o => o.InsertDate))
			{
				Console.WriteLine("{0} - Manual Job: '{1}', next run: ASAP, Enqueued at: '{2}'", now.ToLongTimeString(), etlSchedule.JobName, etlSchedule.InsertDate);
			}
			foreach (var etlSchedule in jobScheduleCollection
				.Where(x => x.Enabled && x.ScheduleType != JobScheduleType.Manual)
				.OrderBy(o => o.TimeToRunNextJob))
			{
				Console.WriteLine("{0} - Scheduled job '{1}', next run: '{2}', Last time started: '{3}'", now.ToLongTimeString(), etlSchedule.JobName, etlSchedule.TimeToRunNextJob, etlSchedule.LastTimeStarted);
			}

			var first = scheduledJobsRunnable.FirstOrDefault();
			if (first != null)
			{
				if (first.ScheduleType == JobScheduleType.Manual)
					Console.WriteLine("{0} - *** Prioritized job to run: Manual Job: '{1}', Enqueued at: '{2}'", now.ToLongTimeString(), first.JobName, first.InsertDate);
				else
					Console.WriteLine("{0} - *** Prioritized job to run '{1}', time: '{2}', Last: '{3}'", now.ToLongTimeString(), first.ScheduleName, first.TimeToRunNextJob, first.LastTimeStarted);
			}

			var count = scheduledJobsRunnable.Count();

			if (count < 2) return first;
			var logText = "There are " + count + "enqueued jobs";
			log.Info(logText);

			return first;
		}

	}
}