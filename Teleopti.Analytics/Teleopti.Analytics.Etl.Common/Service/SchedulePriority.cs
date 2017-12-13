using System;
using System.Linq;
using log4net;
using Microsoft.AnalysisServices;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;

namespace Teleopti.Analytics.Etl.Common.Service
{
	public class SchedulePriority
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(SchedulePriority));

		public IEtlJobSchedule GetTopPriority(IEtlJobScheduleCollection jobScheduleCollection, DateTime now, DateTime serviceStartTime)
		{
			var jobSchedules = jobScheduleCollection
				.Where(x => x.ScheduleType == JobScheduleType.Manual && x.Enabled)
				.OrderBy(x => x.InsertDate);

			if (!jobSchedules.Any())
			{
				jobSchedules = from s in jobScheduleCollection
					where s.Enabled
						  && s.TimeToRunNextJob > s.LastTimeStarted
						  && s.TimeToRunNextJob < now
						  && s.TimeToRunNextJob > serviceStartTime
						  && (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt || s.ScheduleType == JobScheduleType.OccursDaily)
					orderby s.TimeToRunNextJob
					select s;
			}
			

			foreach (var etlSchedule in jobScheduleCollection)
			{
				if (etlSchedule.ScheduleType != JobScheduleType.Manual)
					Console.WriteLine("Schedule: Next: {0} / Last: {1}", etlSchedule.TimeToRunNextJob, etlSchedule.LastTimeStarted);
				else
					Console.WriteLine("Schedule: Next: Manual Job: {0} Enqueued at {1}", etlSchedule.JobName, etlSchedule.InsertDate);
			}

			var first = jobSchedules.FirstOrDefault();
			if (first!=null)
			{
				if(first.ScheduleType != JobScheduleType.Manual)
					Console.WriteLine("Overdue: Next: {0} / Last: {1}", first.TimeToRunNextJob, first.LastTimeStarted);
				else
					Console.WriteLine("Overdue: Manual Job: {0} / Enqueued at: {1}", first.JobName, first.InsertDate);
			}

			var count = jobSchedules.Count();

			if (count < 2) return first;
			var logText = "There are " + count + "enqueued jobs";
			log.Info(logText);

			return first;
		}

	}
}