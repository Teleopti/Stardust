using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class JobCollectionModelProvider
	{
		private readonly IComponentContext _componentContext;
		private readonly IPmInfoProvider _pmInfoProvider;
		private readonly IConfigurationHandler _configurationHandler;
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;

		public JobCollectionModelProvider(
			IComponentContext componentContext, 
			IPmInfoProvider pmInfoProvider, 
			IConfigurationHandler configurationHandler, 
			AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor)
		{
			_componentContext = componentContext;
			_pmInfoProvider = pmInfoProvider;
			_configurationHandler = configurationHandler;
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
		}
		public IList<JobCollectionModel> Create(string tenantName)
		{
			IJobParameters jobParameters;
			if (Tenants.IsAllTenants(tenantName))
			{
				// For "<All>" tenants, just create a dummy JobParameters
				jobParameters = new JobParameters(null, 0, TimeZone.CurrentTimeZone.StandardName, 15, "", "",
					CultureInfo.CurrentCulture, new IocContainerHolder(_componentContext), true);
			}
			else
			{
				var dataMartConnectionString = _analyticsConnectionsStringExtractor.Extract(tenantName);

				_configurationHandler.SetConnectionString(dataMartConnectionString);
				jobParameters = new JobParameters(null, 1,
					_configurationHandler.BaseConfiguration.TimeZoneCode,
					_configurationHandler.BaseConfiguration.IntervalLength.Value,
					_pmInfoProvider.Cube(), _pmInfoProvider.PmInstallation(),
					CultureInfo.GetCultureInfo(_configurationHandler.BaseConfiguration.CultureId.Value),
					new IocContainerHolder(_componentContext),
					_configurationHandler.BaseConfiguration.RunIndexMaintenance);
			}

			var jobCollection = new JobCollection(jobParameters);

			return jobCollection
				.Select(m => new JobCollectionModel
				{
					JobName = m.Name,
					JobStepNames = m.StepList
						.Select(y => y.Name)
						.ToList(),
					NeedsParameterDataSource = m.NeedsParameterDataSource,
					NeededDatePeriod = getJobCategoryPeriods(m)
				})
				.ToList();
		}

		private IList<string> getJobCategoryPeriods(IJob job)
		{
			var returnList = new List<string>();
			if (job.Name == "Intraday")
				return returnList;
			foreach (var jobCategoryType in job.JobCategoryCollection)
			{
				if (jobCategoryType == JobCategoryType.DoNotNeedDatePeriod)
					continue;

				if (job.Name != "Initial" && jobCategoryType == JobCategoryType.Initial)
					continue;

				returnList.Add(jobCategoryType.ToString());
			}

			return returnList;
		}
	}
}