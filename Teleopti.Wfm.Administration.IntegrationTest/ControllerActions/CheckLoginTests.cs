using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class CheckLoginTests
	{

		public DatabaseController Target;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public TestPollutionCleaner TestPollutionCleaner;

		[Test]
		public void ShouldReturnFalseIfLoginExist()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "alogin", "password", new SqlVersion(12, false));
			
			var model = new CreateTenantModel
			{
				AppUser = "alogin",
				AppPassword = "Passw0rd",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "password"
			};

			var result = Target.CheckLogin(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The login already exists you must create a new one.");
		}

		[Test]
		public void ShouldReturnFalseIfPasswordNotStrong()
		{
			var model = new CreateTenantModel
			{
				AppUser = "alogin",
				AppPassword = "password",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "Passw0rd"
			};

			var result = Target.CheckLogin(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().StartWith("Make sure you have entered a strong Password.");
		}

		[Test]
		public void ShouldReturnTrueIfCorrectNewLogin()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			TestPollutionCleaner.Clean("tenant", "appuser");

			var model = new CreateTenantModel
			{
				AppUser = RandomName.Make(),
				AppPassword = "S0meG0odpassword",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "password"
			};

			var result = Target.CheckLogin(model).Content;
			result.Success.Should().Be.True();

		}
	}
}
