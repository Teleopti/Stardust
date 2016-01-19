using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common
{
	public class TenantInfo
	{
		public string Name { get { return Tenant.Name; } }
		public Tenant Tenant { get; set; }
		public IDataSource DataSource { get; set; }
		public IBaseConfiguration EtlConfiguration { get; set; }
	}

	public class TenantsLoadedInEtl : IAllTenantNames
	{
		private readonly Tenants _tenants;

		public TenantsLoadedInEtl(Tenants tenants)
		{
			_tenants = tenants;
		}

		public IEnumerable<string> Tenants()
		{
			return _tenants
				.CurrentTenants()
				.Select(x => x.Name)
				.ToArray();
		}
	}

	public class Tenants
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private IEnumerable<TenantInfo> _tenants = Enumerable.Empty<TenantInfo>();
		private bool _tenantsLoaded = false;

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

		public IEnumerable<TenantInfo> LoadedTenants()
		{
			ensureTenantsLoaded();
			return _tenants.ToList();
		}

		public IEnumerable<TenantInfo> CurrentTenants()
		{
			refresh();
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
			return _tenants.FirstOrDefault(x => x.Tenant.Name.Equals(name));
		}

		public IDataSource DataSourceForTenant(string name)
		{
			var tenant = Tenant(name);
			return tenant == null ? null : tenant.DataSource;
		}

		private void ensureTenantsLoaded()
		{
			if (_tenantsLoaded) return;
			_tenantsLoaded = true;
			refresh();
		}

		private void refresh()
		{
			var refreshed = _tenants.ToList();

			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = _loadAllTenants.Tenants().ToList();
				foreach (var tenant in loaded)
				{
					var existing = _tenants.FirstOrDefault(x => x.Name.Equals(tenant.Name));
					if (existing != null) continue;

					var configurationHandler = new ConfigurationHandler(new GeneralFunctions(tenant.DataSourceConfiguration.AnalyticsConnectionString, _baseConfigurationRepository));
					IBaseConfiguration baseConfiguration = null;
					if (configurationHandler.IsConfigurationValid)
						baseConfiguration = configurationHandler.BaseConfiguration;

					var dataSource = _dataSourcesFactory.Create(
						tenant.Name,
						tenant.DataSourceConfiguration.ApplicationConnectionString,
						tenant.DataSourceConfiguration.AnalyticsConnectionString);

					refreshed.Add(new TenantInfo
					{
						Tenant = tenant,
						EtlConfiguration = baseConfiguration,
						DataSource = dataSource
					});
				}

				var toRemove = (from t in _tenants
								where loaded.FirstOrDefault(x => x.Name.Equals(t.Name)) == null
								select t).ToList();
				foreach (var t in toRemove)
				{
					refreshed.Remove(t);
				}

				_tenants = refreshed;
			}
		}

	}

}