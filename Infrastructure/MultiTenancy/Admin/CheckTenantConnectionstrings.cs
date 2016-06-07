using System;
using System.Data.SqlClient;
using log4net;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.Admin
{
	public class CheckTenantConnectionstrings
	{
		private readonly ILoadAllTenants _loadAllTenants;
		private readonly ICurrentTenantSession _tenantUnitOfWorkManager;
		private static readonly ILog log = LogManager.GetLogger(typeof(CheckTenantConnectionstrings));
		public CheckTenantConnectionstrings(ILoadAllTenants loadAllTenants, ICurrentTenantSession tenantUnitOfWorkManager)
		{
			_loadAllTenants = loadAllTenants;
			_tenantUnitOfWorkManager = tenantUnitOfWorkManager;
		}

		public bool CheckEm(string tenantStoreConnectionstring)
		{
			log.DebugFormat("Checking tenant connection strings [{0}]", tenantStoreConnectionstring);
			var storeConn = new SqlConnectionStringBuilder(tenantStoreConnectionstring);
			var tenants = _loadAllTenants.Tenants();
			foreach (var tenant in tenants)
			{
				var tenantConn = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
				log.DebugFormat("Comparing server {0} and database {1} to connectionstring {2}.", tenantConn.DataSource, tenantConn.InitialCatalog, tenantStoreConnectionstring);
				if (tenantConn.DataSource.Equals(storeConn.DataSource, StringComparison.OrdinalIgnoreCase) &&
					  tenantConn.InitialCatalog.Equals(storeConn.InitialCatalog, StringComparison.OrdinalIgnoreCase))
					return true;
				if (tenantConn.DataSource.Equals("tcp:" + storeConn.DataSource, StringComparison.OrdinalIgnoreCase) &&
					  tenantConn.InitialCatalog.Equals(storeConn.InitialCatalog, StringComparison.OrdinalIgnoreCase))
					return true;
			}
			foreach (var tenant in tenants)
			{
				var tenantConn = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString)
				{
					DataSource = "XXXChangeConnectionstrings"
				};
				log.DebugFormat("Changing Data Source for Tenant {0} to make sure we don't patch wrong database.", tenant.Name);
				tenant.DataSourceConfiguration.SetApplicationConnectionString(tenantConn.ConnectionString);
				_tenantUnitOfWorkManager.CurrentSession().Save(tenant);
			}
			return false;
		}
	}
}