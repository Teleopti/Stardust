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
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.ControllerActions
{
	[WfmAdminTest]
	public class ImportTenantTest
	{
		public ImportController Target;
		public TestPollutionCleaner TestPollutionCleaner;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ILoadAllTenants LoadAllTenants;
		public ITenantUnitOfWork TenantUnitOfWork;

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotExists()
		{
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
				{
					UserID = "appuser",
					Password = "passwordPassword7",
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
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
				{
					InitialCatalog = Guid.NewGuid().ToString(),
					UserID = "appuser",
					Password = "passwordPassword7",
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
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "passwordPassword7";
			
			var sqlVersion = new SqlVersion(12);
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "passwordPassword7");
			DatabaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCC7, "appuser", "passwordPassword7", sqlVersion,
				"NewFineTenant", 1);

			var builderAnal = TestPollutionCleaner.TestTenantAnalyticsConnection();
			builderAnal.IntegratedSecurity = false;
			builderAnal.UserID = "dbcreatorperson";
			builderAnal.Password = "passwordPassword7";

			DatabaseHelperWrapper.CreateDatabase(builderAnal.ConnectionString, DatabaseType.TeleoptiAnalytics, "appuser", "passwordPassword7", sqlVersion, "NewFineTenant", 1);

			var tempModel = new CreateTenantModelForTest();
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			var importModel = new ImportDatabaseModel
			{
				Server = connStringBuilder.DataSource,
				AdminUser = tempModel.CreateDbUser,
				AdminPassword = tempModel.CreateDbPassword,
				UserName = "appuser",
				Password = "passwordPassword7",
				AppDatabase = TestPollutionCleaner.TestTenantConnection().InitialCatalog,
				AnalyticsDatabase = TestPollutionCleaner.TestTenantAnalyticsConnection().InitialCatalog,
				Tenant = "NewFineTenant"
			};

			var result = Target.ImportExisting(importModel);
			result.Content.Success.Should().Be.EqualTo(true);
			using (TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				LoadAllTenants.Tenants().FirstOrDefault(x => x.Name.Equals("NewFineTenant")).Should().Not.Be.EqualTo(null);
				LoadAllTenants.Tenants().Single(x => x.Name == "NewFineTenant").RtaKey.Should().Not.Be.Null();
			}

			appConnString(TestPollutionCleaner.TestTenantConnection().ConnectionString).Should().Contain("NOT IN USE");

		}

		[Test]
		public void ShouldReturnFalseIfImportingSameDatabaseAgain()
		{
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "passwordPassword7";
			
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "passwordPassword7");

			var tempModel = new CreateTenantModelForTest();
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			var importModel = new ImportDatabaseModel
			{
				Server = connStringBuilder.DataSource,
				AdminUser = tempModel.CreateDbUser,
				AdminPassword = tempModel.CreateDbPassword,
				UserName = "appuser",
				Password = "passwordPassword7",
				UseIntegratedSecurity = true,
				AppDatabase = connStringBuilder.InitialCatalog,
				AnalyticsDatabase = connStringBuilder.InitialCatalog,
				Tenant = "NewFineTenant"
			};

			var result = Target.ImportExisting(importModel);
			result.Content.Success.Should().Be.EqualTo(false);

			result.Content.Message.Should().Contain("not allowed");

		}
		private string appConnString(string  connectionString)
		{
			var retString = "";
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format("select ApplicationConnectionString from Tenant.Tenant");
					var reader = cmd.ExecuteReader();
					if (!reader.HasRows) return retString;
					reader.Read();
					retString = reader.GetString(0);
				}
				conn.Close();
			}
			
			return retString;
		}
	}
}