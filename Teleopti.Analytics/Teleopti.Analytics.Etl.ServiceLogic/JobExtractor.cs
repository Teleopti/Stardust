using System;
using System.Globalization;
using log4net;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	internal static class JobExtractor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static IJob ExtractJobFromSchedule(IEtlSchedule etlScheduleToRun, JobHelper jobHelper, string timeZoneId, int intervalLengthMinutes, string cube, string pmInstallation)
		{
			var log = LogManager.GetLogger(typeof(JobExtractor));
			log.InfoFormat(CultureInfo.InvariantCulture, "Getting job to run from schedule '{0}'.", etlScheduleToRun.ScheduleName);

			var jobParameters =
				new JobParameters(GetJobCategoryDatePeriods(etlScheduleToRun, timeZoneId),
								  etlScheduleToRun.DataSourceId, timeZoneId,
								  intervalLengthMinutes,
								  cube,
								  pmInstallation,
								  CultureInfo.CurrentCulture)
					{
						Helper = jobHelper
					};

			var jobCollection = new JobCollection(jobParameters);

			IJob jobToRun = null;

			foreach (var job in jobCollection)
			{
				if (String.Compare(job.Name, etlScheduleToRun.JobName, StringComparison.Ordinal) == 0)
					jobToRun = job;
			}

			if (jobToRun == null) throw new Exception("Job name in schedule does not exist");

			log.InfoFormat(CultureInfo.InvariantCulture, "Job to run is '{0}'", jobToRun.Name);
			return jobToRun;
		}

		private static IJobMultipleDate GetJobCategoryDatePeriods(IEtlSchedule etlSchedule, string timeZoneId)
		{
			IJobMultipleDate jobMultipleDate = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			var today = DateTime.Today;

			foreach (IEtlJobRelativePeriod jobRelativePeriod in etlSchedule.RelativePeriodCollection)
			{
				jobMultipleDate.Add(today.AddDays(jobRelativePeriod.RelativePeriod.Minimum),
									today.AddDays(jobRelativePeriod.RelativePeriod.Maximum),
									jobRelativePeriod.JobCategory);
			}

			return jobMultipleDate;
		}
	}
}