using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class UpgradeDatabase
	{
		public UpgradeDatabasesController UpgradeDatabasesController;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public TestPollutionCleaner TestPollutionCleaner;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;

		[Test]
		public void ShouldUgradeToLatestVersion()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());

			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "passwordPassword7";
			
			var helper = new DatabaseHelper(builder.ConnectionString, DatabaseType.TeleoptiCCC7);
			helper.CreateByDbManager();
			
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString,"appuser", "passwordPassword7");

			var builderAnal = TestPollutionCleaner.TestTenantAnalyticsConnection();
			builderAnal.IntegratedSecurity = false;
			builderAnal.UserID = "dbcreatorperson";
			builderAnal.Password = "passwordPassword7";

			helper = new DatabaseHelper(builderAnal.ConnectionString, DatabaseType.TeleoptiAnalytics);
			helper.CreateByDbManager();

			builder.UserID = "appuser";
			builder.Password = "passwordPassword7";
			var appConn = builder.ConnectionString;

			builder.InitialCatalog = TestPollutionCleaner.TestTenantAnalyticsConnection().InitialCatalog;

			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("NewOne");
				tenant.DataSourceConfiguration.SetApplicationConnectionString(appConn);
				tenant.DataSourceConfiguration.SetAnalyticsConnectionString(builder.ConnectionString);
				CurrentTenantSession.CurrentSession().Save(tenant);
			}

			UpgradeDatabasesController.UpgradeTenant(new UpgradeTenantModel
			{
				Tenant = "NewOne",
				AdminUserName = "dbcreatorperson",
				AdminPassword = "passwordPassword7"
			});

		}
	}
}