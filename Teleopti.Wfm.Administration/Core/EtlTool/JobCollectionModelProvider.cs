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
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class JobCollectionModelProvider
	{
		private const string reloadDatamartJobName = "Reload datamart (old nightly)";
		private const string nightlyJobName = "Nightly";
		private const string permissionJobName = "Permission";
		private const string insightsDataRefreshJobName = "Insights data refresh";

		private readonly IComponentContext _componentContext;
		private readonly IPmInfoProvider _pmInfoProvider;
		private readonly IConfigurationHandler _configurationHandler;
		private readonly IConfigReader _configReader;
		private readonly ITenants _tenants;
		private readonly AnalyticsConnectionsStringExtractor _analyticsConnectionsStringExtractor;

		public JobCollectionModelProvider(
			IComponentContext componentContext,
			IPmInfoProvider pmInfoProvider,
			IConfigurationHandler configurationHandler,
			IConfigReader configReader,
			ITenants tenants,
			AnalyticsConnectionsStringExtractor analyticsConnectionsStringExtractor)
		{
			_componentContext = componentContext;
			_pmInfoProvider = pmInfoProvider;
			_configurationHandler = configurationHandler;
			_configReader = configReader;
			_tenants = tenants;
			_analyticsConnectionsStringExtractor = analyticsConnectionsStringExtractor;
		}

		public IList<JobCollectionModel> Create(string tenantName)
		{
			var isAllTenants = Tenants.IsAllTenants(tenantName);
			var dataMartConnectionString = isAllTenants
				? _configReader.ConnectionString("Hangfire")
				: _analyticsConnectionsStringExtractor.Extract(tenantName);
			_configurationHandler.SetConnectionString(dataMartConnectionString);

			var baseConfiguration = _configurationHandler.BaseConfiguration;
			var jobParameters = new JobParameters(null, 1,
				baseConfiguration.TimeZoneCode,
				baseConfiguration.IntervalLength.Value,
				_pmInfoProvider.Cube(),
				_pmInfoProvider.PmInstallation(),
				CultureInfo.GetCultureInfo(baseConfiguration.CultureId.Value),
				new IocContainerHolder(_componentContext),
				baseConfiguration.RunIndexMaintenance,
				insightsEnabled(tenantName, baseConfiguration));

			return new JobCollection(jobParameters)
				.Select(job => new JobCollectionModel
				{
					JobName = job.Name,
					JobSteps = job.StepList.Select(step => new JobStepModel
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
			if (jobName == reloadDatamartJobName &&
				(stepName == "AnalyticsIndexMaintenance" || stepName == "AppIndexMaintenance" ||
				 stepName == "AggIndexMaintenance"))
			{
				return true;
			}

			if ((jobName == nightlyJobName || jobName == permissionJobName || jobName == reloadDatamartJobName) &&
				(stepName == "Process Cube" || stepName == "Performance Manager permissions"))
			{
				return true;
			}

			if ((jobName == nightlyJobName || jobName == insightsDataRefreshJobName) &&
				(stepName == "Trigger Insights data refresh"))
			{
				return true;
			}

			return false;
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

		private bool insightsEnabled(string tenantName, IBaseConfiguration baseConfiguration)
		{
			if (Tenants.IsAllTenants(tenantName))
			{
				var allTenants = _tenants.LoadedTenants();
				return allTenants.Any(x => insightsLicensed(x.Name));
			}

			return insightsLicensed(tenantName) && (baseConfiguration.InsightsConfig?.IsValid() ?? false);
		}

		private bool insightsLicensed(string tenantName)
		{
			var licenseActivator = DefinedLicenseDataFactory.GetLicenseActivator(tenantName);
			var licensed = licenseActivator?.EnabledLicenseOptionPaths?
									   .Contains(DefinedLicenseOptionPaths.TeleoptiWfmInsights) ?? false;
			return licensed;
		}
	}
}