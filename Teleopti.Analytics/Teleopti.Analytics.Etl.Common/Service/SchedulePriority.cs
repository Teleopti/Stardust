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
						  && (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt ||
							  s.ScheduleType == JobScheduleType.OccursDaily)
					orderby s.TimeToRunNextJob
					select s;

				scheduledJobsRunnable = jobs.ToList();
			}

			foreach (var etlSchedule in jobScheduleCollection
				.Where(x => x.Enabled && x.ScheduleType == JobScheduleType.Manual)
				.OrderBy(o => o.InsertDate))
			{
				Console.WriteLine($@"{now.ToLongTimeString()} - Manual Job: '{etlSchedule.JobName}', next run: ASAP, Enqueued at: '{etlSchedule.InsertDate}'");
			}

			foreach (var etlSchedule in jobScheduleCollection
				.Where(x => x.Enabled && x.ScheduleType != JobScheduleType.Manual)
				.OrderBy(o => o.TimeToRunNextJob))
			{
				Console.WriteLine($@"{now.ToLongTimeString()} - [{etlSchedule.ScheduleName}] Scheduled job '{etlSchedule.JobName}', next run: '{etlSchedule.TimeToRunNextJob}', Last time started: '{etlSchedule.LastTimeStarted}'");
			}

			var first = scheduledJobsRunnable.FirstOrDefault();
			if (first != null)
			{
				var message = first.ScheduleType == JobScheduleType.Manual
					? $@"{now.ToLongTimeString()} - *** Prioritized job to run: Manual Job: '{first.JobName}', Enqueued at: '{first.InsertDate}'"
					: $@"{now.ToLongTimeString()} - *** Prioritized job to run: Job schedule: '{first.ScheduleName}', time: '{first.TimeToRunNextJob}', Last: '{first.LastTimeStarted}'";

				Console.WriteLine(message);
			}

			var count = scheduledJobsRunnable.Count;
			if (count < 2) return first;

			var logText = "There are " + count + "enqueued jobs";
			log.Info(logText);

			return first;
		}
	}
}