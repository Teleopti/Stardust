using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class EtlToolJobCollectionModelProvider
	{
		private readonly IComponentContext _componentContext;
		private readonly PmInfoProvider _pmInfoProvider;
		private readonly IConfigurationHandler _configurationHandler;
		private readonly ILoadAllTenants _loadAllTenants;

		public EtlToolJobCollectionModelProvider(IComponentContext componentContext, PmInfoProvider pmInfoProvider, IConfigurationHandler configurationHandler, ILoadAllTenants loadAllTenants)
		{
			_componentContext = componentContext;
			_pmInfoProvider = pmInfoProvider;
			_configurationHandler = configurationHandler;
			_loadAllTenants = loadAllTenants;
		}
		public IList<JobCollectionModel> Create(string tenantName)
		{
			/*_configurationHandler.SetConnectionString("Ten");
			IJobParameters jobParameters = new JobParameters(null, 1,
				_configurationHandler.BaseConfiguration.TimeZoneCode,
				_configurationHandler.BaseConfiguration.IntervalLength.Value,
				_pmInfoProvider.Cube(), _pmInfoProvider.PmInstallation(), 
				CultureInfo.GetCultureInfo(_configurationHandler.BaseConfiguration.CultureId.Value), 
				new IocContainerHolder(_componentContext),
				_configurationHandler.BaseConfiguration.RunIndexMaintenance);
			var jobCollection = new JobCollection(jobParameters);

			return jobCollection
				.Select(m => new JobCollectionModel
				{
					JobName = m.Name ,
					JobStepNames = m.StepList
						.Select(y => y.Name)
						.ToList()
				})
				.ToList();*/
			return null;
		}
	}
}