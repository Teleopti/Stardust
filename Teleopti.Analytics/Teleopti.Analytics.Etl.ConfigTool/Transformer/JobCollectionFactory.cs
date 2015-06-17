using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Config;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class JobCollectionFactory
	{
		private readonly IBaseConfiguration _baseConfiguration;
		private readonly IContainer _container;

		public JobCollectionFactory(IBaseConfiguration baseConfiguration, IContainer container)
		{
			_baseConfiguration = baseConfiguration;
			_container = container;
		}

		public ObservableCollection<IJob> JobCollection
		{
			get
			{
				var jobParameters = new JobParameters(
					null, 1,
					_baseConfiguration.TimeZoneCode,
					_baseConfiguration.IntervalLength.Value,
					ConfigurationManager.AppSettings["cube"],
					ConfigurationManager.AppSettings["pmInstallation"],
					CultureInfo.CurrentCulture,
					new IocContainerHolder(_container), 
					_baseConfiguration.RunIndexMaintenance
					);
				var path = ConfigurationManager.AppSettings["nhibConfPath"];
				if (string.IsNullOrEmpty(path))
					path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				var databaseConfigurationReader = new ReadDataSourceConfigurationFromNhibFiles(new NhibFilePathFixed(path), new ParseNhibFile());

				_baseConfiguration.JobHelper = new JobHelper(databaseConfigurationReader);
				jobParameters.Helper = _baseConfiguration.JobHelper;

				var jobCollection = new JobCollection(jobParameters);

				var jobs = new ObservableCollection<IJob>(jobCollection);

				return jobs;
			}
		}
	}
}
