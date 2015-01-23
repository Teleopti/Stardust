using System;
using System.Globalization;
using log4net;
using Teleopti.Analytics.Etl.Interfaces.Common;
using Teleopti.Analytics.Etl.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Transformer.Job;
using Teleopti.Analytics.Etl.Transformer.Job.Jobs;
using Teleopti.Analytics.Etl.Transformer.Job.MultipleDate;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.ServiceLogic
{
	internal static class JobExtractor
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static IJob ExtractJobFromSchedule(
			IEtlJobSchedule etlJobScheduleToRun, JobHelper jobHelper, 
			string timeZoneId, int intervalLengthMinutes, string cube, 
			string pmInstallation, IToggleManager toggleManager)
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
					CultureInfo.CurrentCulture, 
					toggleManager,
					false)
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