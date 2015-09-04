using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Support.Security;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantUpgrader
	{
		private readonly DatabaseUpgrader _databaseUpgrader;
		private readonly UpgradeRunner _upgradeRunner;

		public TenantUpgrader(DatabaseUpgrader databaseUpgrader, UpgradeRunner upgradeRunner)
		{
			_databaseUpgrader = databaseUpgrader;
			_upgradeRunner = upgradeRunner;
		}

		public void Upgrade(Tenant tenant, string adminUserName, string adminPassword, bool permissionMode)
		{
			//dbmanager
			var builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.ApplicationConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCC7, adminUserName,
				adminPassword, builder.UserID, builder.Password, permissionMode);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = false;
			var appConnstring = builder.ConnectionString;
			builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AnalyticsConnectionString);
			_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiAnalytics, adminUserName,
				adminPassword, builder.UserID, builder.Password, permissionMode);
			builder.UserID = adminUserName;
			builder.Password = adminPassword;
			builder.IntegratedSecurity = false;
			var analConnstring = builder.ConnectionString;
			// and agg to 

			//security 
			var dbArgs = new DatabaseArguments
			{
				ApplicationDbConnectionString = appConnstring,
				ApplicationDbConnectionStringToStore = appConnstring,
				AnalyticsDbConnectionString = analConnstring,
				AnalyticsDbConnectionStringToStore = analConnstring,
				AggDatabase = "main_clone_DemoSales_TeleoptiCCCAgg",
			};
			_upgradeRunner.Upgrade(dbArgs);

		}
	}
}