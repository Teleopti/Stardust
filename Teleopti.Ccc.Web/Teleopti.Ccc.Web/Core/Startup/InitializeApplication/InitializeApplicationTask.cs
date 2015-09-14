using System.Configuration;
using System.Threading.Tasks;
using Owin;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Web.Core.Startup.Booter;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core.Startup.InitializeApplication
{
	[TaskPriority(5)]
	public class InitializeApplicationTask : IBootstrapperTask
	{
		private readonly IInitializeApplication _initializeApplication;
		private readonly ISettings _settings;
		private readonly IPhysicalApplicationPath _physicalApplicationPath;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public InitializeApplicationTask(IInitializeApplication initializeApplication, 
												ISettings settings, 
												IPhysicalApplicationPath physicalApplicationPath, 
												ITenantUnitOfWork tenantUnitOfWork,
												ILoadAllTenants loadAllTenants,
												IDataSourceForTenant dataSourceForTenant)
		{
			_initializeApplication = initializeApplication;
			_settings = settings;
			_physicalApplicationPath = physicalApplicationPath;
			_tenantUnitOfWork = tenantUnitOfWork;
			_loadAllTenants = loadAllTenants;
			_dataSourceForTenant = dataSourceForTenant;
		}

		public Task Execute(IAppBuilder application)
		{
			var passwordPolicyPath = System.IO.Path.Combine(_physicalApplicationPath.Get(), _settings.ConfigurationFilesPath());
			using (_tenantUnitOfWork.Start())
			{
				_initializeApplication.Start(new WebState(), new LoadPasswordPolicyService(passwordPolicyPath), ConfigurationManager.AppSettings.ToDictionary(), false);
				//remove when tenants are handled correctly in RTA
				_loadAllTenants.Tenants().ForEach(dsConf =>
				{
					_dataSourceForTenant.MakeSureDataSourceExists(dsConf.Name,
						dsConf.DataSourceConfiguration.ApplicationConnectionString,
						dsConf.DataSourceConfiguration.AnalyticsConnectionString,
						dsConf.DataSourceConfiguration.ApplicationNHibernateConfig);
				});
				/////////////////////////////////////////////////
			}
			return null;
		}
	}
}