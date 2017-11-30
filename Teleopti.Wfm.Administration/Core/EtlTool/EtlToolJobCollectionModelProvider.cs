using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using Autofac;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core.EtlTool
{
	public class EtlToolJobCollectionModelProvider
	{
		private readonly IComponentContext _componentContext;

		public EtlToolJobCollectionModelProvider(IComponentContext componentContext)
		{
			_componentContext = componentContext;
		}
		public IList<JobCollectionModel> Create()
		{
			var configurationHandler = new ConfigurationHandler(new GeneralFunctions(ConfigurationManager.ConnectionStrings["Hangfire"].ConnectionString, new BaseConfigurationRepository()));
			var pmInfoProvider = _componentContext.Resolve<PmInfoProvider>();
			IJobParameters jobParameters = new JobParameters(null, 1, 
				configurationHandler.BaseConfiguration.TimeZoneCode, 
				configurationHandler.BaseConfiguration.IntervalLength.Value,
				pmInfoProvider.Cube(), pmInfoProvider.PmInstallation(), 
				CultureInfo.GetCultureInfo(configurationHandler.BaseConfiguration.CultureId.Value), 
				new IocContainerHolder(_componentContext), 
				configurationHandler.BaseConfiguration.RunIndexMaintenance);
			var jobCollection = new JobCollection(jobParameters);

			return jobCollection.Select(m => new JobCollectionModel{JobName = m.Name , JobStepNames = m.StepList.Select(y => y.Name).ToList()}).ToList();
		}
	}
}