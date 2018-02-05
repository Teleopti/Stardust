using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Analytics.Etl.Common
{
	public interface ITenants
	{
		IEnumerable<TenantInfo> LoadedTenants();
		IEnumerable<TenantInfo> CurrentTenants();
		IEnumerable<TenantInfo> EtlTenants();
		TenantInfo Tenant(string name);
		IDataSource DataSourceForTenant(string name);
	}

}