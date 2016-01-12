using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using Autofac;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Interfaces.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
using Teleopti.Analytics.Etl.Common.Transformer.Job.Jobs;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

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
				var jobParameters = new JobParameters(
					null, 1,
					_baseConfiguration.TimeZoneCode,
					_baseConfiguration.IntervalLength.Value,
					ConfigurationManager.AppSettings["cube"],
					ConfigurationManager.AppSettings["pmInstallation"],
					CultureInfo.CurrentCulture,
					new IocContainerHolder(App.Container), 
					_baseConfiguration.RunIndexMaintenance
					);

				_baseConfiguration.JobHelper = new JobHelper(
					App.Container.Resolve<ILoadAllTenants>(),
					App.Container.Resolve<ITenantUnitOfWork>(),
					App.Container.Resolve<IAvailableBusinessUnitsProvider>()
					);

				jobParameters.Helper = _baseConfiguration.JobHelper;

				var jobCollection = new JobCollection(jobParameters);

				var jobs = new ObservableCollection<IJob>(jobCollection);

				return jobs;
			}
		}
	}
}
