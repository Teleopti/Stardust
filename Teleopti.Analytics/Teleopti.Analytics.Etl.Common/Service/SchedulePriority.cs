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

			var scheulesOverDue = from s in jobScheduleCollection
								  where s.Enabled
										&& s.TimeToRunNextJob > s.LastTimeStarted
										&& s.TimeToRunNextJob < now
										&& s.TimeToRunNextJob > serviceStartTime 
										&& (s.PeriodicStartingTodayAt < now && now < s.PeriodicEndingTodayAt || s.ScheduleType == JobScheduleType.OccursDaily)
								  orderby s.TimeToRunNextJob
								  select s;

			foreach (var etlSchedule in jobScheduleCollection)
			{
				Console.WriteLine("Schedule: Next: {0} / Last: {1}", etlSchedule.TimeToRunNextJob, etlSchedule.LastTimeStarted);
			}

			var first = scheulesOverDue.FirstOrDefault();
			if (first!=null)
			{
				Console.WriteLine("Overdue: Next: {0} / Last: {1}", first.TimeToRunNextJob, first.LastTimeStarted);
			}

			var count = scheulesOverDue.Count();
			if (count >= 2)
			{
				var logText = "There are " + count + "enqueued jobs";
				log.Info(logText);
			}

			return first;
		}

	}
}