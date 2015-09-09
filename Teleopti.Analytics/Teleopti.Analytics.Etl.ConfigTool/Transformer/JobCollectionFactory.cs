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
using Teleopti.Ccc.Infrastructure.UnitOfWork;

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
				_baseConfiguration.JobHelper = new JobHelper(_container.Resolve<ILoadAllTenants>(),
					_container.Resolve<ITenantUnitOfWork>(), _container.Resolve<IAvailableBusinessUnitsProvider>(),
					_container.Resolve<IDataSourcesFactory>());
				jobParameters.Helper = _baseConfiguration.JobHelper;

				var jobCollection = new JobCollection(jobParameters);

				var jobs = new ObservableCollection<IJob>(jobCollection);

				return jobs;
			}
		}
	}
}
