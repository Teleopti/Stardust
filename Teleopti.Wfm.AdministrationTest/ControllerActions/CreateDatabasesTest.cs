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

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class CreateDatabasesTest
	{
		public DatabaseController Target;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;
		public TestPolutionCleaner TestPolutionCleaner;

		[Test]
		public void ShouldReturnSuccessFalseIfFirstUserIsEmpty()
		{
			var model = new CreateTenantModel { AppUser = "user", AppPassword = "password" };
			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user name can not be empty.");
		}

		[Test]
		public void ShouldReturnSuccessFalseIfBusinessUnitIsEmpty()
		{
			var model = new CreateTenantModel {FirstUser = "thefirst", FirstUserPassword = "password", AppUser = "user", AppPassword = "password",  };
			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The Business Unit can not be empty.");
		}
		[Test]
		public void ShouldReturnSuccessFalseIfTenantExists()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			using (TenantUnitOfWork.Start())
			{
				var tenant = new Tenant("Old One");
				CurrentTenantSession.CurrentSession().Save(tenant);
			}
			var model = new CreateTenantModel { Tenant = "Old One" };
			bool result = Target.CreateDatabases(model).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfWrongCredentials()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");


			var model = new CreateTenantModel { Tenant = "New Tenant",CreateDbUser = "dummy", CreateDbPassword = "dummy"};

			bool result = Target.CreateDatabases(model).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnFalseIfNotDbCreator()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

            DatabaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString,"nodbcreator", "password", false);

			var model = new CreateTenantModel { Tenant = "New Tenant", CreateDbUser = "nodbcreator", CreateDbPassword = "password", AppUser = "user", AppPassword = "password", FirstUser = "user", FirstUserPassword = "password", BusinessUnit = "BU"};

			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user does not have permission to create databases.");
		}

		[Test]
		public void ShouldReturnTrueCreatedDb()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TestPolutionCleaner.Clean("New Tenant", "new TenantAppUser");

			var result = Target.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "New Tenant",
				AppUser = "new TenantAppUser",
			});

			result.Content.Success.Should().Be.True();
		}

	}
}