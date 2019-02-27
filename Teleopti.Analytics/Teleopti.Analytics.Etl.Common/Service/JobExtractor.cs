using System;
using System.Globalization;
using System.Linq;
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
	public class JobExtractor
	{
		private readonly IComponentContext _componentContext;
		private static readonly ILog log = LogManager.GetLogger(typeof(JobExtractor));

		public JobExtractor(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}

		public IJob ExtractJobFromSchedule(
			IEtlJobSchedule etlJobScheduleToRun, 
			JobHelper jobHelper, 
			string timeZoneId, 
			int intervalLengthMinutes, 
			string cube,
			string pmInstallation, 
			bool runIndexMaintenance, 
			bool insightsEnabled,
			CultureInfo culture)
		{
			var jobName = etlJobScheduleToRun.JobName;
			var scheduleName = etlJobScheduleToRun.ScheduleName;
			log.InfoFormat(CultureInfo.InvariantCulture, "Getting job to run from schedule '{0}'.", scheduleName);

			var jobParameters =
				new JobParameters(
					getJobCategoryDatePeriods(etlJobScheduleToRun, timeZoneId),
					etlJobScheduleToRun.DataSourceId, timeZoneId,
					intervalLengthMinutes,
					cube,
					pmInstallation,
					culture,
					new IocContainerHolder(_componentContext), 
					runIndexMaintenance,
					insightsEnabled)
					{
						Helper = jobHelper
					};

			var jobToRun = new JobCollection(jobParameters).SingleOrDefault(job =>
				string.Compare(job.Name, jobName, StringComparison.Ordinal) == 0);

			if (jobToRun == null)
			{
				throw new Exception($"Job name '{jobName}' in schedule '{scheduleName}' does not exist");
			}

			log.InfoFormat(CultureInfo.InvariantCulture, "Job to run is '{0}'", jobToRun.Name);
			return jobToRun;
		}

		private static IJobMultipleDate getJobCategoryDatePeriods(IEtlJobSchedule etlJobSchedule, string timeZoneId)
		{
			IJobMultipleDate jobMultipleDate = new JobMultipleDate(TimeZoneInfo.FindSystemTimeZoneById(timeZoneId));
			var today = DateTime.Today;

			foreach (var jobRelativePeriod in etlJobSchedule.RelativePeriodCollection)
			{
				jobMultipleDate.Add(today.AddDays(jobRelativePeriod.RelativePeriod.Minimum),
									today.AddDays(jobRelativePeriod.RelativePeriod.Maximum),
									jobRelativePeriod.JobCategory);
			}

			return jobMultipleDate;
		}
	}
}