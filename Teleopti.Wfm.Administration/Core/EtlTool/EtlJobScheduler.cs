﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Entity;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.JobSchedule;
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
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;

		public EtlJobScheduler(IJobScheduleRepository jobScheduleRepository, 
			IBaseConfigurationRepository baseConfigurationRepository, 
			INow now, 
			AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor)
		{
			_jobScheduleRepository = jobScheduleRepository;
			_baseConfigurationRepository = baseConfigurationRepository;
			_now = now;
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
		}

		public void ScheduleJob(JobEnqueModel jobEnqueModel)
		{
			var connectionString = _analyticsConnectionsStringExtractor.Extract(jobEnqueModel.TenantName);
			var baseConfig = _baseConfigurationRepository.LoadBaseConfiguration(connectionString);
			IList<IEtlJobRelativePeriod> relativePeriodCollection =
				GetRelativePeriods(jobEnqueModel, baseConfig.TimeZoneCode);
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

		public IList<IEtlJobRelativePeriod> GetRelativePeriods(JobEnqueModel job, string timeZoneId)
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
				var jobCategory = Enum.GetValues(typeof(JobCategoryType)).Cast<JobCategoryType>().First(x => x.ToString() == jobPeriod.JobCategoryName);
				var localStart = TimeZoneHelper.ConvertFromUtc(jobPeriod.Start, timeZone);
				var localEnd = TimeZoneHelper.ConvertFromUtc(jobPeriod.End, timeZone);
				var relativeStart = localStart.Date.Subtract(localNow.Date).Days;
				var relativeEnd = localEnd.Date.Subtract(localNow.Date).Days;
				var relativePeriod = new MinMax<int>(relativeStart, relativeEnd);
				ret.Add(new EtlJobRelativePeriod(relativePeriod, jobCategory));
			}
			return ret;
		}
	}
}