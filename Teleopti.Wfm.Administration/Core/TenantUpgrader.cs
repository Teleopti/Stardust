using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Azure;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Library;
using Teleopti.Support.Security;
using Teleopti.Support.Security.Library;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantUpgrader
	{
		private readonly DatabaseUpgrader _databaseUpgrader;
		private readonly UpgradeRunner _upgradeRunner;
		private readonly ITenantUnitOfWork _tenantUnitOfWork;
		private readonly ICurrentTenantSession _currentTenantSession;

		public TenantUpgrader(
			DatabaseUpgrader databaseUpgrader, 
			UpgradeRunner upgradeRunner, 
			ITenantUnitOfWork tenantUnitOfWork, 
			ICurrentTenantSession currentTenantSession)
		{
			_databaseUpgrader = databaseUpgrader;
			_upgradeRunner = upgradeRunner;
			_tenantUnitOfWork = tenantUnitOfWork;
			_currentTenantSession = currentTenantSession;
		}

		public void Upgrade(Tenant tenant, string adminUserName, string adminPassword, bool permissionMode, bool useIntegratedSecurity)
		{
			var aggDB = "";
			if (string.IsNullOrEmpty(adminUserName))
				adminUserName = "";
			if (string.IsNullOrEmpty(adminPassword))
				adminPassword = "";
			//dbmanager
			var builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCC7, adminUserName,
				adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name, tenant.Id);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = useIntegratedSecurity;
			var appConnstring = builder.ConnectionString;
			builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AnalyticsConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiAnalytics, adminUserName,
				adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name, tenant.Id);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = useIntegratedSecurity;
			var analConnstring = builder.ConnectionString;
			aggDB = builder.InitialCatalog;
			// and agg to
			if (!AzureCommon.IsAzure && !string.IsNullOrEmpty(tenant.DataSourceConfiguration.AggregationConnectionString))
			{
				builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AggregationConnectionString);
				_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCCAgg, adminUserName,
					adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name, tenant.Id);
				builder.UserID = adminUserName;
				builder.Password = adminPassword;
				builder.IntegratedSecurity = useIntegratedSecurity;
				aggDB = builder.InitialCatalog;
			}

			//security 
			var upgradeCommand = new UpgradeCommand
			{
				ApplicationConnectionString = appConnstring,
				ApplicationConnectionStringToStore = appConnstring,
				AnalyticsConnectionString = analConnstring,
				AnalyticsConnectionStringToStore = analConnstring,
				AggDatabase = aggDB
			};
			_upgradeRunner.SetLogger(new TenantLogger(tenant.Name, tenant.Id));

			_upgradeRunner.Upgrade(upgradeCommand, _tenantUnitOfWork, _currentTenantSession);
		}
	}
}