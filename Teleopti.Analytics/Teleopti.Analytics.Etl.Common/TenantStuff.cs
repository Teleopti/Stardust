using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Analytics.Etl.Common.Transformer.Job;
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
		private readonly JobHelper _jobHelper;

		public Tenants(JobHelper jobHelper)
		{
			_jobHelper = jobHelper;
		}

		public IEnumerable<TenantInfo> CurrentTenants()
		{
			_jobHelper.RefreshTenantList();
			return _jobHelper.TenantCollection;
		}
	}
	
	public class TenantHolder
	{
		private IEnumerable<TenantInfo> _tenants = Enumerable.Empty<TenantInfo>();
		
		public IEnumerable<TenantInfo> LoadedTenants()
		{
			return _tenants;
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

		public void Refresh(ITenantUnitOfWork tenantUnitOfWork, ILoadAllTenants loadAllTenants)
		{
			var dataSourcesFactory = new DataSourcesFactory(
				new EnversConfiguration(),
				new NoPersistCallbacks(),
				DataSourceConfigurationSetter.ForEtl(),
				new CurrentHttpContext(),
				() => StateHolderReader.Instance.StateReader.ApplicationScopeData.Messaging
				);

			var refreshed = _tenants.ToList();

			using (tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var loaded = loadAllTenants.Tenants().ToList();
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