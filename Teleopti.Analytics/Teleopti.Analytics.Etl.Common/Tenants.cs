using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.Common
{
	public class Tenants : ITenants
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;

		public Tenants(
			ITenantUnitOfWork tenantUnitOfWork, 
			ILoadAllTenants loadAllTenants, 
			IDataSourcesFactory dataSourcesFactory,
			IBaseConfigurationRepository baseConfigurationRepository)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_loadAllTenants = loadAllTenants;
			_dataSourcesFactory = dataSourcesFactory;
			_baseConfigurationRepository = baseConfigurationRepository;
		}

		public IEnumerable<TenantInfo> CurrentTenants()
		{
			return LoadedTenants();
		}

		public IEnumerable<TenantInfo> EtlTenants()
		{
			return LoadedTenants()
				.Where(t => t.EtlConfiguration != null)
				.ToList();
		}

		public TenantInfo Tenant(string name)
		{
			return LoadedTenants().FirstOrDefault(x => x.Tenant.Name.Equals(name));
		}

		public IDataSource DataSourceForTenant(string name)
		{
			var tenant = Tenant(name);
			return tenant?.DataSource;
		}

		public IEnumerable<TenantInfo> LoadedTenants()
		{
			var loadedTenants = new List<TenantInfo>();

			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = _loadAllTenants.Tenants().ToList();
				foreach (var tenant in loaded)
				{
					var configurationHandler = new ConfigurationHandler(new GeneralFunctions(new GeneralInfrastructure(_baseConfigurationRepository)));
					configurationHandler.SetConnectionString(tenant.DataSourceConfiguration.AnalyticsConnectionString);
					IBaseConfiguration baseConfiguration = null;
					if (configurationHandler.IsConfigurationValid)
						baseConfiguration = configurationHandler.BaseConfiguration;

					var dataSource = _dataSourcesFactory.Create(
						tenant.Name,
						tenant.DataSourceConfiguration.ApplicationConnectionString,
						tenant.DataSourceConfiguration.AnalyticsConnectionString);

					loadedTenants.Add(new TenantInfo
					{
						Tenant = tenant,
						EtlConfiguration = baseConfiguration,
						DataSource = dataSource
					});
				}
			}

			return loadedTenants;
		}
	}
}