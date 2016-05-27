using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class CheckTenantConnectionstrings
	{
		private readonly LoadAllTenants _loadAllTenants;
		private readonly TenantUnitOfWorkManager _tenantUnitOfWorkManager;

		public CheckTenantConnectionstrings(LoadAllTenants loadAllTenants, TenantUnitOfWorkManager tenantUnitOfWorkManager)
		{
			_loadAllTenants = loadAllTenants;
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public bool CheckEm(string tenantStoreConnectionstring)
		{
			var storeConn = new SqlConnectionStringBuilder(tenantStoreConnectionstring);
			var tenants = _loadAllTenants.Tenants();
			foreach (var tenant in tenants)
			{
				var tenantConn = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
				if (tenantConn.DataSource.Equals(storeConn.DataSource, StringComparison.OrdinalIgnoreCase) &&
					 tenantConn.InitialCatalog.Equals(storeConn.InitialCatalog, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			foreach (var tenant in tenants)
			{
				var tenantConn = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString)
				{
					DataSource = "XXXChangeConnectionstrings"
				};
				tenant.DataSourceConfiguration.SetApplicationConnectionString(tenantConn.ConnectionString);
				_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			}

			return false;
		}
	}
}