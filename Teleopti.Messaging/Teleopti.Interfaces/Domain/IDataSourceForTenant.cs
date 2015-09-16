using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface IDataSourceForTenant : IDisposable
	{
		IDataSource Tenant(string tenantName);
		void MakeSureDataSourceExists(string tenantName, string applicationConnectionString, string analyticsConnectionString, IDictionary<string, string> applicationNhibConfiguration);
		void DoOnAllTenants_AvoidUsingThis(Action<IDataSource> actionOnTenant);
		void RemoveDataSource(string tenantName);
	}
}