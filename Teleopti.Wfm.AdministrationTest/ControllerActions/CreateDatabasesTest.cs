using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
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
			var model = new CreateTenantModelForTest
			{
				BusinessUnit = "BU",
				AppUser = "somenewnotthere",
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
				FirstUser = "somenewnotthere"
			};
         
			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The Business Unit can not be empty.");
		}
		[Test]
		public void ShouldReturnSuccessFalseIfTenantExists()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
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
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");


			var model = new CreateTenantModel { Tenant = "New Tenant",CreateDbUser = "dummy", CreateDbPassword = "dummy"};

			bool result = Target.CreateDatabases(model).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfNotDbCreator()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

            DatabaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString,"nodbcreator", "password", new SqlVersion(12,false));

			var model = new CreateTenantModel { Tenant = "New Tenant", CreateDbUser = "nodbcreator", CreateDbPassword = "password", AppUser = "user", AppPassword = "password", FirstUser = "user", FirstUserPassword = "password", BusinessUnit = "BU"};

			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Contain("Login can not be created.");
		}

		[Test]
		public void ShouldReturnTrueCreatedDb()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(new NoPersistCallbacks(), "TestData");
			TestPollutionCleaner.Clean("New Tenant", "new TenantAppUser");

			var result = Target.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "New Tenant",
				AppUser = "new TenantAppUser",
			});

			result.Content.Success.Should().Be.True();
		}

	}
}