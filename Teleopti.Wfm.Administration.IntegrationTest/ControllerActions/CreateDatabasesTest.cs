using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class CreateDatabasesTest
	{
		public DatabaseController Target;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public TestPollutionCleaner TestPollutionCleaner;

		[Test]
		public void ShouldReturnSuccessFalseIfFirstUserIsEmpty()
		{
			TestPollutionCleaner.Clean("tenant", "appuser");
			var model = new CreateTenantModelForTest
			{
				BusinessUnit = "BU",
				AppUser = "somenewnotthere",
				AppPassword = "Passw0rd",
				FirstUser = ""
			};
			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user name can not be empty.");
		}

		[Test]
		public void ShouldReturnSuccessFalseIfBusinessUnitIsEmpty()
		{
			TestPollutionCleaner.Clean("tenant","appuser");
			var model = new CreateTenantModelForTest
			{
				BusinessUnit = "",
				AppUser = "somenewnotthere",
				AppPassword = "Passw0rd",
				FirstUser = "somenewnotthere"
			};
         
			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The Business Unit can not be empty.");
		}
		[Test]
		public void ShouldReturnSuccessFalseIfTenantExists()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			
			var model = new CreateTenantModelForTest
			{
				Tenant = "Old One",
            BusinessUnit = "BU",
				AppUser = "somenewnotthere",
				FirstUser = "somenewnotthere"
			};
			bool result = Target.CreateDatabases(model).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfWrongCredentials()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());


			var model = new CreateTenantModel { Tenant = "New Tenant",CreateDbUser = "dummy", CreateDbPassword = "dummy"};

			bool result = Target.CreateDatabases(model).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfNotDbCreator()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

            DatabaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString,"nodbcreator", "Password");

			var model = new CreateTenantModel { Tenant = "New Tenant", CreateDbUser = "nodbcreator", CreateDbPassword = "password", AppUser = "user", AppPassword = "Passw0rd", FirstUser = "user", FirstUserPassword = "password", BusinessUnit = "BU"};

			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Contain("Login can not be created.");
		}
		
		[Test]
		public void ShouldReturnTrueCreatedDb()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			TestPollutionCleaner.Clean("New Tenant", "new TenantAppUser");

			var result = Target.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "New Tenant",
				AppUser = "new TenantAppUser",
				AppPassword = "Passw0rd"
			});

			result.Content.Success.Should().Be.True();
		}

	}
}