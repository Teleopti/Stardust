using System.Collections.Generic;
using System.Configuration;
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
using Teleopti.Ccc.Domain.Config;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class JobCollectionModelProvider
	{
		private const string reloadDatamartJobName = "Reload datamart (old nightly)";

		private readonly IComponentContext _componentContext;
		private readonly IPmInfoProvider _pmInfoProvider;
		private readonly IConfigReader _configReader;
		private readonly IConfigurationHandler _configurationHandler;
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;

		public JobCollectionModelProvider(
			IComponentContext componentContext, 
			IPmInfoProvider pmInfoProvider, 
			IConfigReader configReader,
			IConfigurationHandler configurationHandler,
			AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor)
		{
			_componentContext = componentContext;
			_pmInfoProvider = pmInfoProvider;
			_configReader = configReader;
			_configurationHandler = configurationHandler;
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
		}

		public IList<JobCollectionModel> Create(string tenantName)
		{
			var isAllTenants = Tenants.IsAllTenants(tenantName);

			// Get base configuration from main tenant for all tenants
			var dataMartConnectionString = isAllTenants
				? _configReader.ConnectionString("Tenancy")
				: _analyticsConnectionsStringExtractor.Extract(tenantName);

			_configurationHandler.SetConnectionString(dataMartConnectionString);
			var baseConfiguration = _configurationHandler.BaseConfiguration;

			var jobParameters = new JobParameters(null, 1,
				baseConfiguration.TimeZoneCode,
				baseConfiguration.IntervalLength.Value,
				_pmInfoProvider.Cube(), _pmInfoProvider.PmInstallation(),
				CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value),
				new IocContainerHolder(_componentContext),
				baseConfiguration.RunIndexMaintenance);

			var jobCollection = new JobCollection(jobParameters);

			return jobCollection.Select(job => new JobCollectionModel
			{
				JobName = job.Name,
				JobSteps = job.StepList
					.Select(step => new JobStepModel
					{
						Name = step.Name,
						DependsOnTenant = isAllTenants && IsJobStepDependsOnTenant(job.Name, step.Name)
					}).ToList(),
				NeedsParameterDataSource = job.NeedsParameterDataSource,
				NeededDatePeriod = getJobCategoryPeriods(job)
			}).ToList();
		}

		public static bool IsJobStepDependsOnTenant(string jobName, string stepName)
		{
			return jobName == reloadDatamartJobName &&
				   (stepName == "AnalyticsIndexMaintenance" ||
					stepName == "AppIndexMaintenance" ||
					stepName == "AggIndexMaintenance");
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