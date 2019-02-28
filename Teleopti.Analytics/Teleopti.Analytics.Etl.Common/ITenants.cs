using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common
{
	public interface ITenants
	{
		IEnumerable<TenantInfo> LoadedTenants(bool reloadConfiguration = false);
		IEnumerable<TenantInfo> CurrentTenants(bool reloadConfiguration = false);
		IEnumerable<TenantInfo> EtlTenants(bool reloadConfiguration = false);
		TenantInfo Tenant(string name, bool reloadConfiguration = false);
		IDataSource DataSourceForTenant(string name);
	}

}