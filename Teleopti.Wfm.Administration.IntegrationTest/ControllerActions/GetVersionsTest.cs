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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			var appBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			var helper = new DatabaseHelper(appBuilder.ConnectionString, DatabaseType.TeleoptiCCC7);

			TestPollutionCleaner.Clean("tenant", "appuser");

			var sqlVersion = new SqlVersion(12,false);
			helper.LoginTasks().CreateLogin("appuser", "SomeG00dpw", false, sqlVersion);
			helper.AddPermissions("appuser", "SomeG00dpw", sqlVersion);
			var result =
				Target.GetVersions(new VersionCheckModel
				{
					AppDatabase = appBuilder.InitialCatalog,
					Server = appBuilder.DataSource,
					UserName = "appuser",
					Password = "SomeG00dpw"
				});
			result.Content.AppVersionOk.Should().Be.True();
		}
	}
}