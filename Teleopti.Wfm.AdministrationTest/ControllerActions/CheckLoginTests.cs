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

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
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
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "alogin", "password", new SqlVersion(12,false));

			var model = new CreateTenantModel
			{
				AppUser = "alogin",
				AppPassword = "password",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "password"
			};

			var result = Target.CheckLogin(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The login already exists you must create a new one.");
		}

		[Test]
		public void ShouldReturnFalseIfBadPassword()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("tenant", "appuser");
			
			var model = new CreateTenantModel
			{
				AppUser = RandomName.Make(),
				AppPassword = "p",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "password"
			};

			var result = Target.CheckLogin(model).Content;
			result.Success.Should().Be.False();
		}

		[Test]
		public void ShouldReturnTrueIfCorrectNewLogin()
		{
			DataSourceHelper.CreateDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

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
