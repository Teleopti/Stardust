using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.NHibernateConfiguration;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Web;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common
{
	public class TenantInfo
	{
		public string Name { get { return Tenant.Name; } }
		public Tenant Tenant { get; set; }
		public IDataSource DataSource { get; set; }
		public IBaseConfiguration BaseConfiguration { get; set; }
	}
	
	public class Tenants
	{
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;
		private IEnumerable<TenantInfo> _tenants = Enumerable.Empty<TenantInfo>();

		public Tenants(ITenantUnitOfWork tenantUnitOfWork, ILoadAllTenants loadAllTenants)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_loadAllTenants = loadAllTenants;
		}

		public IEnumerable<TenantInfo> LoadedTenants()
		{
			return _tenants.ToList();
		}

		public IEnumerable<TenantInfo> CurrentTenants()
		{
			Refresh();
			return LoadedTenants();
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

		public void Refresh()
		{
			var dataSourcesFactory = new DataSourcesFactory(
				new EnversConfiguration(),
				new NoPersistCallbacks(),
				DataSourceConfigurationSetter.ForEtl(),
				new CurrentHttpContext(),
				() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
				);

			var refreshed = _tenants.ToList();

			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = _loadAllTenants.Tenants().ToList();
				foreach (var tenant in loaded)
				{
					var existing = LoadedTenants().FirstOrDefault(x => x.Name.Equals(tenant.Name));
					if (existing != null) continue;

					var configurationHandler = new ConfigurationHandler(new GeneralFunctions(tenant.DataSourceConfiguration.AnalyticsConnectionString));
					IBaseConfiguration baseConfiguration = null;
					if (configurationHandler.IsConfigurationValid)
						baseConfiguration = configurationHandler.BaseConfiguration;

					var dataSource = dataSourcesFactory.Create(
						tenant.Name,
						tenant.DataSourceConfiguration.ApplicationConnectionString,
						tenant.DataSourceConfiguration.AnalyticsConnectionString);

					refreshed.Add(new TenantInfo
					{
						Tenant = tenant,
						BaseConfiguration = baseConfiguration,
						DataSource = dataSource
					});
				}

				var toRemove = (from t in LoadedTenants()
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