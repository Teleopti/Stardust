using System;
using System.Globalization;
using Autofac;
using log4net;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Common.Transformer.Job.MultipleDate;

namespace Teleopti.Analytics.Etl.Common.Service
{
	internal static class JobExtractor
	{
		public static IJob ExtractJobFromSchedule(
			IEtlJobSchedule etlJobScheduleToRun, 
			JobHelper jobHelper, 
			string timeZoneId, 
			int intervalLengthMinutes, 
			string cube,
			string pmInstallation, 
			IContainer container, 
			bool runIndexMaintenance, 
			CultureInfo culture)
		{
			var log = LogManager.GetLogger(typeof(JobExtractor));
			log.InfoFormat(CultureInfo.InvariantCulture, "Getting job to run from schedule '{0}'.", etlJobScheduleToRun.ScheduleName);

			var jobParameters =
				new JobParameters(
					GetJobCategoryDatePeriods(etlJobScheduleToRun, timeZoneId),
					etlJobScheduleToRun.DataSourceId, timeZoneId,
					intervalLengthMinutes,
					cube,
					pmInstallation,
					culture,
					new IocContainerHolder(container), 
					runIndexMaintenance)
					{
						Helper = jobHelper
					};

			var jobCollection = new JobCollection(jobParameters);

			IJob jobToRun = null;

			foreach (var job in jobCollection)
			{
				if (String.Compare(job.Name, etlJobScheduleToRun.JobName, StringComparison.Ordinal) == 0)
					jobToRun = job;
			}

			if (jobToRun == null) throw new Exception("Job name in schedule does not exist");

			log.InfoFormat(CultureInfo.InvariantCulture, "Job to run is '{0}'", jobToRun.Name);
			return jobToRun;
		}

		private static IJobMultipleDate GetJobCategoryDatePeriods(IEtlJobSchedule etlJobSchedule, string timeZoneId)
		{
			IJobMultipleDate jobMultipleDate = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			var today = DateTime.Today;

			foreach (IEtlJobRelativePeriod jobRelativePeriod in etlJobSchedule.RelativePeriodCollection)
			{
				jobMultipleDate.Add(today.AddDays(jobRelativePeriod.RelativePeriod.Minimum),
									today.AddDays(jobRelativePeriod.RelativePeriod.Maximum),
									jobRelativePeriod.JobCategory);
			}

			return jobMultipleDate;
		}
	}
}