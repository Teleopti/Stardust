using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.MessageBroker.Client;
using Teleopti.Ccc.Domain.Security.Authentication;

namespace Teleopti.Analytics.Etl.ConfigTool.Transformer
{
	public class JobCollectionFactory
	{
		private readonly IBaseConfiguration _baseConfiguration;

		public JobCollectionFactory(IBaseConfiguration baseConfiguration)
		{
			_baseConfiguration = baseConfiguration;
		}

		public ObservableCollection<IJob> JobCollection
		{
			get
			{
				int databaseTimeoutInSecond;
				if (!int.TryParse(ConfigurationManager.AppSettings["databaseTimeout"], out databaseTimeoutInSecond))
				{
					databaseTimeoutInSecond = 60;
				}

				var jobParameters = new JobParameters(
					null, 1,
					_baseConfiguration.TimeZoneCode,
					_baseConfiguration.IntervalLength.Value,
					ConfigurationManager.AppSettings["cube"],
					ConfigurationManager.AppSettings["pmInstallation"],
					CultureInfo.CurrentCulture,
					new IocContainerHolder(App.Container),
					_baseConfiguration.RunIndexMaintenance
					)
				{
					DatabaseTimeoutInSecond = databaseTimeoutInSecond
				};

				_baseConfiguration.JobHelper = new JobHelper(
					App.Container.Resolve<IAvailableBusinessUnitsProvider>(),
					App.Container.Resolve<Tenants>(),
					App.Container.Resolve<IIndexMaintenanceRepository>(),
					App.Container.Resolve<IMessageSender>());

				jobParameters.Helper = _baseConfiguration.JobHelper;

				var jobCollection = new JobCollection(jobParameters);

				var jobs = new ObservableCollection<IJob>(jobCollection);

				return jobs;
			}
		}
	}
}