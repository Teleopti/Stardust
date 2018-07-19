using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Infrastructure;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Analytics.Etl.Common.Transformer;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.Toggle;
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
		private readonly IToggleManager _toggleManager;
		private static readonly Dictionary<string, TenantInfo> allTenants = new Dictionary<string, TenantInfo>();

		public Tenants(
			ITenantUnitOfWork tenantUnitOfWork, 
			ILoadAllTenants loadAllTenants, 
			IDataSourcesFactory dataSourcesFactory,
			IBaseConfigurationRepository baseConfigurationRepository,
			IToggleManager toggleManager)
		{
			_tenantUnitOfWork = tenantUnitOfWork;
			_loadAllTenants = loadAllTenants;
			_dataSourcesFactory = dataSourcesFactory;
			_baseConfigurationRepository = baseConfigurationRepository;
			_toggleManager = toggleManager;
		}

		public static bool IsAllTenants(string tenantName)
		{
			return tenantName == NameForOptionAll;
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
			var toggle76761Enabled = _toggleManager.IsEnabled(Toggles.ETL_Optimize_Memory_Usage_76761);

			using (_tenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var generalInfrastructure = new GeneralInfrastructure(_baseConfigurationRepository);
				var generalFunctions = new GeneralFunctions(generalInfrastructure);
				var configurationHandler = new ConfigurationHandler(generalFunctions, new BaseConfigurationValidator());

				var loaded = _loadAllTenants.Tenants().ToList();
				foreach (var tenant in loaded)
				{
					if (!toggle76761Enabled)
					{
						var tenantInfo = getTenantInfo(tenant, configurationHandler);
						loadedTenants.Add(tenantInfo);
					}
					else
					{
						if (allTenants.ContainsKey(tenant.Name)) continue;

						var tenantInfo = getTenantInfo(tenant, configurationHandler);
						allTenants.Add(tenant.Name, tenantInfo);
					}
				}
			}

			return toggle76761Enabled ? allTenants.Values.ToList() : loadedTenants;
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