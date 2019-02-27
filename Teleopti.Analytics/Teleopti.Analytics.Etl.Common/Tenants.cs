using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Analytics.Etl.Common
{
	public class Tenants : ITenants
	{
		public const string NameForOptionAll = "<All>";
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly IDataSourcesFactory _dataSourcesFactory;
		private readonly IBaseConfigurationRepository _baseConfigurationRepository;
		private static readonly Dictionary<string, TenantInfo> allTenants = new Dictionary<string, TenantInfo>();

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

		public static bool IsAllTenants(string tenantName)
		{
			return tenantName == NameForOptionAll;
		}

		public IEnumerable<TenantInfo> CurrentTenants(bool reloadConfiguration = false)
		{
			return LoadedTenants(reloadConfiguration);
		}

		public IEnumerable<TenantInfo> EtlTenants(bool reloadConfiguration = false)
		{
			return LoadedTenants(reloadConfiguration)
				.Where(t => t.EtlConfiguration != null)
				.ToList();
		}

		public TenantInfo Tenant(string name, bool reloadConfiguration = false)
		{
			return LoadedTenants(reloadConfiguration)
				.Where(t => t.EtlConfiguration != null)
				.FirstOrDefault(x => x.Tenant.Name.Equals(name));
		}

		public IDataSource DataSourceForTenant(string name)
		{
			var tenant = Tenant(name);
			return tenant?.DataSource;
		}

		public IEnumerable<TenantInfo> LoadedTenants(bool reloadConfiguration = false)
		{
			Dictionary<string, TenantInfo> retList;
			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var generalInfrastructure = new GeneralInfrastructure(_baseConfigurationRepository);
				var generalFunctions = new GeneralFunctions(generalInfrastructure);
				var configurationHandler = new ConfigurationHandler(generalFunctions, new BaseConfigurationValidator());

				var tenantsInApp = _loadAllTenants.Tenants().ToList();
				foreach (var tenant in tenantsInApp)
				{
					if (allTenants.ContainsKey(tenant.Name))
					{
						if (reloadConfiguration || allTenants[tenant.Name].EtlConfiguration == null)
						{
							allTenants[tenant.Name] = getTenantInfo(tenant, configurationHandler);
						}
						continue;
					}

					var tenantInfo = getTenantInfo(tenant, configurationHandler);
					allTenants.Add(tenant.Name, tenantInfo);
				}

				retList = allTenants.Where(x => tenantsInApp.Any(y => y.Name == x.Value.Tenant.Name))
					.ToDictionary(x => x.Key, x => x.Value);
			}

			return retList.Values.ToList();
		}

		private TenantInfo getTenantInfo(Tenant tenant, IConfigurationHandler configurationHandler)
		{
			var dataSourceConfiguration = tenant.DataSourceConfiguration;
			configurationHandler.SetConnectionString(dataSourceConfiguration.AnalyticsConnectionString);
			var baseConfiguration = configurationHandler.IsConfigurationValid
				? configurationHandler.BaseConfiguration
				: null;

			var dataSource = _dataSourcesFactory.Create(tenant.Name,
				dataSourceConfiguration.ApplicationConnectionString,
				dataSourceConfiguration.AnalyticsConnectionString);

			var tenantInfo = new TenantInfo
			{
				Tenant = tenant,
				EtlConfiguration = baseConfiguration,
				DataSource = dataSource
			};
			return tenantInfo;
		}
	}
}