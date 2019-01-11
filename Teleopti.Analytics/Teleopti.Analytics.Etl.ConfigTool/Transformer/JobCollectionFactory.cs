using System.Collections.ObjectModel;
using System.Globalization;
using Autofac;
using Teleopti.Analytics.Etl.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Service;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Domain.Analytics.Transformer;
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
				var pmInfoProvider = App.Container.Resolve<PmInfoProvider>();
				var jobParameters = new JobParameters(
					null, 1,
					_baseConfiguration.TimeZoneCode,
					_baseConfiguration.IntervalLength.Value,
					pmInfoProvider.Cube(),
					pmInfoProvider.PmInstallation(),
					CultureInfo.CurrentCulture,
					new IocContainerHolder(App.Container),
					_baseConfiguration.RunIndexMaintenance,
					_baseConfiguration.InsightsConfig?.IsValid() ?? false
				);

				_baseConfiguration.JobHelper = new JobHelper(
					App.Container.Resolve<IAvailableBusinessUnitsProvider>(),
					App.Container.Resolve<ITenants>(),
					App.Container.Resolve<IIndexMaintenanceRepository>(),
					App.Container.Resolve<IMessageSender>(),
					App.Container.Resolve<IAnalyticsPersonPeriodDateFixer>());

				jobParameters.Helper = _baseConfiguration.JobHelper;
				var jobCollection = new JobCollection(jobParameters);
				var jobs = new ObservableCollection<IJob>(jobCollection);
				return jobs;
			}
		}
	}
}
