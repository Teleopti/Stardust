using System.Collections.Generic;
using System.Linq;
using Teleopti.Analytics.Etl.Common.Interfaces.Common;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Analytics.Etl.Common.Infrastructure
{
	public sealed class TenantHolder
	{
		public static TenantHolder Instance
		{
			get
			{
				return Nested.instance;
			}
		}

		class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly TenantHolder instance = new TenantHolder();
		}

		public void SetTenantBaseConfigs(IList<TenantBaseConfig> tenantBaseConfigs )
		{
			TenantBaseConfigs = tenantBaseConfigs;
		}

		public IDataSource TenantDataSource(string name)
		{
			return TenantBaseConfigs.FirstOrDefault(x => x.Tenant.Name.Equals(name)).TenantDataSource;
		} 
		public IList<TenantBaseConfig> TenantBaseConfigs{ get; private set; }
	}

	public class TenantBaseConfig
	{
		public Tenant Tenant { get; set; }
		public IBaseConfiguration BaseConfiguration { get; set; }

		public IDataSource TenantDataSource { get; set; }
	}
}