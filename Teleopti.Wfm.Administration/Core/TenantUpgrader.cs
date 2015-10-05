using System;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Support.Security;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantUpgrader
	{
		private readonly bool isAzure =  !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));
		private readonly DatabaseUpgrader _databaseUpgrader;
		private readonly UpgradeRunner _upgradeRunner;

		public TenantUpgrader(DatabaseUpgrader databaseUpgrader, UpgradeRunner upgradeRunner)
		{
			_databaseUpgrader = databaseUpgrader;
			_upgradeRunner = upgradeRunner;
		}

		public void Upgrade(Tenant tenant, string adminUserName, string adminPassword, bool permissionMode, bool useIntegratedSecurity)
		{
			//dbmanager
			var builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCC7, adminUserName,
				adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = false;
			var appConnstring = builder.ConnectionString;
			builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AnalyticsConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiAnalytics, adminUserName,
				adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = false;
			var analConnstring = builder.ConnectionString;
			// and agg to
			if (!isAzure && !string.IsNullOrEmpty(tenant.DataSourceConfiguration.AggregationConnectionString))
			{
				builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AggregationConnectionString);
				_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCCAgg, adminUserName,
					adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name);
				builder.UserID = adminUserName;
				builder.Password = adminPassword;
				builder.IntegratedSecurity = false;
			}
			
			//security 
			var dbArgs = new DatabaseArguments
			{
				ApplicationDbConnectionString = appConnstring,
				ApplicationDbConnectionStringToStore = appConnstring,
				AnalyticsDbConnectionString = analConnstring,
				AnalyticsDbConnectionStringToStore = analConnstring,
				AggDatabase = builder.ConnectionString,
			};
			_upgradeRunner.Logger = new TenantLogger(tenant.Name);
         _upgradeRunner.Upgrade(dbArgs);

		}
	}
}