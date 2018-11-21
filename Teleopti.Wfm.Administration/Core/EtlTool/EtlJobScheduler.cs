using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobSchedule;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class EtlJobScheduler
	{
		private readonly IJobScheduleRepository _jobScheduleRepository;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private readonly INow _now;
		private readonly IConfigReader _configReader;

		public EtlJobScheduler(IJobScheduleRepository jobScheduleRepository,
			IBaseConfigurationRepository baseConfigurationRepository,
			INow now,
			IConfigReader configReader)
		{
			_jobScheduleRepository = jobScheduleRepository;
			_baseConfigurationRepository = baseConfigurationRepository;
			_now = now;
			_configReader = configReader;
		}

		public void ScheduleManualJob(EtlJobEnqueModel jobEnqueModel)
		{
			var connectionString = _configReader.ConnectionString("Hangfire");

			var baseConfig = _baseConfigurationRepository.LoadBaseConfiguration(connectionString);
			var relativePeriodCollection = getSchedulePeriodsForEnqueuedJob(jobEnqueModel, baseConfig.TimeZoneCode);

			var etlJobSchedule = new EtlJobSchedule(
				-1,
				"Manual ETL",
				jobEnqueModel.JobName,
				true,
				jobEnqueModel.LogDataSourceId,
				"Manual ETL",
				DateTime.MinValue,
				relativePeriodCollection,
				jobEnqueModel.TenantName);

			_jobScheduleRepository.SetDataMartConnectionString(connectionString);
			var scheduleId = _jobScheduleRepository.SaveSchedule(etlJobSchedule);
			etlJobSchedule.SetScheduleIdOnPersistedItem(scheduleId);
			_jobScheduleRepository.SaveSchedulePeriods(etlJobSchedule);
		}
		
		public IList<IEtlJobSchedule> LoadScheduledJobs()
		{
			_jobScheduleRepository.SetDataMartConnectionString(_configReader.ConnectionString("Hangfire"));
			return _jobScheduleRepository.GetSchedules(null, DateTime.Now);
		}

		public void ScheduleDailyJob(EtlScheduleJobModel scheduleModel)
		{
			var relativePeriodCollection = getSchedulePeriods(scheduleModel);

			var etlJobSchedule = new EtlJobSchedule(
				scheduleModel.ScheduleId,
				scheduleModel.ScheduleName,
				scheduleModel.Enabled,
				(int) scheduleModel.DailyFrequencyStart.TimeOfDay.TotalMinutes,
				scheduleModel.JobName,
				scheduleModel.LogDataSourceId,
				scheduleModel.Description,
				null,
				relativePeriodCollection,
				scheduleModel.Tenant);

			_jobScheduleRepository.SetDataMartConnectionString(_configReader.ConnectionString("Hangfire"));
			var scheduleId = _jobScheduleRepository.SaveSchedule(etlJobSchedule);
			etlJobSchedule.SetScheduleIdOnPersistedItem(scheduleId);
			_jobScheduleRepository.SaveSchedulePeriods(etlJobSchedule);
		}

		private IList<IEtlJobRelativePeriod> getSchedulePeriodsForEnqueuedJob(EtlJobEnqueModel job, string timeZoneId)
		{
			var ret = new List<IEtlJobRelativePeriod>();
			if (job.JobName == "Intraday")
			{
				ret.Add(new EtlJobRelativePeriod(new MinMax<int>(0, 0), JobCategoryType.AgentStatistics));
				ret.Add(new EtlJobRelativePeriod(new MinMax<int>(0, 0), JobCategoryType.QueueStatistics));
				return ret;
			}

			var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
			var localNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), timeZone);

			foreach (var jobPeriod in job.JobPeriods)
			{
				var jobCategory = Enum.GetValues(typeof(JobCategoryType)).Cast<JobCategoryType>()
					.First(x => x.ToString() == jobPeriod.JobCategoryName);
				var relativeStart = jobPeriod.Start.Date.Subtract(localNow.Date).Days;
				var relativeEnd = jobPeriod.End.Date.Subtract(localNow.Date).Days;
				var relativePeriod = new MinMax<int>(relativeStart, relativeEnd);
				ret.Add(new EtlJobRelativePeriod(relativePeriod, jobCategory));
			}

			return ret;
		}

		private static IList<IEtlJobRelativePeriod> getSchedulePeriods(EtlScheduleJobModel scheduleModel)
		{
			var ret = new List<IEtlJobRelativePeriod>();
			if (scheduleModel.JobName == "Intraday")
			{
				ret.Add(new EtlJobRelativePeriod(new MinMax<int>(0, 0), JobCategoryType.AgentStatistics));
				ret.Add(new EtlJobRelativePeriod(new MinMax<int>(0, 0), JobCategoryType.QueueStatistics));
				return ret;
			}

			foreach (var period in scheduleModel.RelativePeriods)
			{
				var jobCategory = Enum.GetValues(typeof(JobCategoryType)).Cast<JobCategoryType>()
					.First(x => x.ToString() == period.JobCategoryName);
				ret.Add(new EtlJobRelativePeriod(new MinMax<int>(period.Start, period.End), jobCategory));	
			}
			return ret;
		}

		public void SchedulePeriodicJob(EtlScheduleJobModel scheduleModel)
		{
			var relativePeriodCollection = getSchedulePeriods(scheduleModel);

			var etlJobSchedule = new EtlJobSchedule(
				scheduleModel.ScheduleId,
				scheduleModel.ScheduleName,
				scheduleModel.Enabled,
				Convert.ToInt32(scheduleModel.DailyFrequencyMinute),
				(int)scheduleModel.DailyFrequencyStart.TimeOfDay.TotalMinutes,
				(int)scheduleModel.DailyFrequencyEnd.TimeOfDay.TotalMinutes,
				scheduleModel.JobName,
				scheduleModel.LogDataSourceId,
				scheduleModel.Description,
				null,
				DateTime.MinValue, 
				relativePeriodCollection,
				scheduleModel.Tenant);

			_jobScheduleRepository.SetDataMartConnectionString(_configReader.ConnectionString("Hangfire"));
			var scheduleId = _jobScheduleRepository.SaveSchedule(etlJobSchedule);
			etlJobSchedule.SetScheduleIdOnPersistedItem(scheduleId);
			_jobScheduleRepository.SaveSchedulePeriods(etlJobSchedule);
		}

		public void ToggleScheduleJobEnabledState(int scheduleId)
		{
			_jobScheduleRepository.SetDataMartConnectionString(_configReader.ConnectionString("Hangfire"));
			_jobScheduleRepository.ToggleScheduleJobEnabledState(scheduleId);
		}

		public void DeleteScheduleJob(int scheduleId)
		{
			_jobScheduleRepository.SetDataMartConnectionString(_configReader.ConnectionString("Hangfire"));
			_jobScheduleRepository.DeleteSchedule(scheduleId);
		}
	}
}
