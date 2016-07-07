﻿using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Support.Security;

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
			// and agg to
			if (!isAzure() && !string.IsNullOrEmpty(tenant.DataSourceConfiguration.AggregationConnectionString))
			{
				builder = new SqlConnectionStringBuilder(tenant.DataSourceConfiguration.AggregationConnectionString);
				_databaseUpgrader.Upgrade(builder.DataSource, builder.InitialCatalog, DatabaseType.TeleoptiCCCAgg, adminUserName,
					adminPassword, useIntegratedSecurity, builder.UserID, builder.Password, permissionMode, tenant.Name, tenant.Id);
				builder.UserID = adminUserName;
				builder.Password = adminPassword;
				builder.IntegratedSecurity = useIntegratedSecurity;
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
			_upgradeRunner.Logger = new TenantLogger(tenant.Name, tenant.Id);

			_upgradeRunner.UpgradeProcess(_tenantUnitOfWork, _currentTenantSession, dbArgs);
		}

		private bool isAzure()
		{
			var tennConn = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			return tennConn.DataSource.Contains("database.windows.net");
		}
	}
}