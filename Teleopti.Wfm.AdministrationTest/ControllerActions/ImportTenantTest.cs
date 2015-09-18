using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class ImportTenantTest
	{
		public ImportController Target;
		public TestPolutionCleaner TestPolutionCleaner;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ILoadAllTenants LoadAllTenants;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotExists()
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				UserID = "appuser",
				Password = "password",
				IntegratedSecurity = false
			};

			var importModel = new ImportDatabaseModel
			{
				Server = connStringBuilder.DataSource,
				UserName = connStringBuilder.UserID,
				Password = connStringBuilder.Password,
				AppDatabase = connStringBuilder.InitialCatalog,
				AnalyticsDatabase = RandomName.Make()
			};
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.False();
		}

		[Test]
		public void ShouldReturnSuccessFalseIfAppDatabaseNotExists()
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				InitialCatalog = Guid.NewGuid().ToString(),
				UserID = "appuser",
				Password = "password",
				IntegratedSecurity = false
			};

			var importModel = new ImportDatabaseModel
			{
				Server = connStringBuilder.DataSource,
				UserName = connStringBuilder.UserID,
				Password = connStringBuilder.Password,
				AppDatabase = RandomName.Make(),
				AnalyticsDatabase = connStringBuilder.InitialCatalog
			};
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.False();
		}

		[Test]
		public void ShouldImportExistingDatabases()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TestPolutionCleaner.Clean("tenant", "appuser");
			var builder = TestPolutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "password";

			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "password", false);
			DatabaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCC7,"","appuser",false, "NewFineTenant");
			
			var builderAnal = TestPolutionCleaner.TestTenantAnalyticsConnection();
			builderAnal.IntegratedSecurity = false;
			builderAnal.UserID = "dbcreatorperson";
			builderAnal.Password = "password";

			DatabaseHelperWrapper.CreateDatabase(builderAnal.ConnectionString, DatabaseType.TeleoptiAnalytics, "", "appuser", false, "NewFineTenant");

			var tempModel = new CreateTenantModelForTest();
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

         var importModel = new ImportDatabaseModel
			{
				Server = connStringBuilder.DataSource,
				AdminUser = tempModel.CreateDbUser,
				AdminPassword = tempModel.CreateDbPassword,
				UserName = "appuser",
				Password = "password",
				AppDatabase = TestPolutionCleaner.TestTenantConnection().InitialCatalog,
				AnalyticsDatabase = TestPolutionCleaner.TestTenantAnalyticsConnection().InitialCatalog, 
				Tenant = "NewFineTenant"
			};

			var result = Target.ImportExisting(importModel);
			result.Content.Success.Should().Be.EqualTo(true);
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals("NewFineTenant")).Should().Not.Be.EqualTo(null);
				LoadAllTenants.Tenants().Single(x => x.Name == "NewFineTenant").RtaKey.Should().Not.Be.Null();
			}
		}
	}
}