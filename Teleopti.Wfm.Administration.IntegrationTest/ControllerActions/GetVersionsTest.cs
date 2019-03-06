using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class GetVersionsTest
	{
		public UpgradeDatabasesController Target;
		public TestPollutionCleaner TestPollutionCleaner;

		[Test]
		public void ShouldReportOkIfSameVersion()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceFactoryFactory.MakeLegacyWay());
			var appBuilder = new SqlConnectionStringBuilder(InfraTestConfigReader.ApplicationConnectionString());
			var helper = new DatabaseHelper(appBuilder.ConnectionString, DatabaseType.TeleoptiCCC7, new WfmInstallationEnvironment());

			TestPollutionCleaner.Clean("tenant", "appuser");

			var sqlVersion = new SqlVersion(12);
			helper.LoginTasks().CreateLogin("appuser", "SomeG00dpwPassword", false);
			helper.AddPermissions("appuser", "SomeG00dpwPassword", sqlVersion);
			var result =
				Target.GetVersions(new VersionCheckModel
				{
					AppDatabase = appBuilder.InitialCatalog,
					Server = appBuilder.DataSource,
					UserName = "appuser",
					Password = "SomeG00dpwPassword"
				});
			result.Content.AppVersionOk.Should().Be.True();
		}
	}
}