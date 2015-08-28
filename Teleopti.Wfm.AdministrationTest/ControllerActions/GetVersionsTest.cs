using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GetVersionsTest
	{
		public UpgradeDatabasesController Target;
		public TestPolutionCleaner TestPolutionCleaner;

		[Test]
		public void ShouldReportOkIfSameVersion()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			var appBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			var helper = new DatabaseHelper(appBuilder.ConnectionString, DatabaseType.TeleoptiCCC7);

			TestPolutionCleaner.Clean("tenant", "appuser");

			helper.CreateLogin("appuser", "SomeG00dpw", false);
			helper.AddPermissions("appuser");
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