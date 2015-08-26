using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class GenerateRtaKeyForTenant
	{
		public DatabaseController Database;
		public ImportController Import;
		public ILoadAllTenants Tenants;
		public ITenantUnitOfWork TenantUnitOfWork;
		public TestPolutionCleaner TestPolutionCleaner;

		[Test]
		public void ShouldGenerateRtaKeyWhenCreatingDatabases()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TestPolutionCleaner.Clean("tenant", "appuser");

			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant",
				AppUser = "appuser",
			});

			using (TenantUnitOfWork.Start())
				Tenants.Tenants().Single(x => x.Name == "tenant").RtaKey.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGenerateRtaKeyWhenImporting()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TestPolutionCleaner.Clean("tenant", "appuser");

			var connString = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString) { InitialCatalog = TestPolutionCleaner.TestTenantDatabaseName }.ConnectionString;
			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			if (!helper.Exists())
			{
				helper.CreateByDbManager();
				helper.CreateSchemaByDbManager();
			}

			var connStringAnal = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString) { InitialCatalog = TestPolutionCleaner.TestTenantAnalyticsDatabaseName }.ConnectionString;
			var helperAnal = new DatabaseHelper(connStringAnal, DatabaseType.TeleoptiAnalytics);
			if (!helperAnal.Exists())
			{
				helperAnal.CreateByDbManager();
				helperAnal.CreateSchemaByDbManager();
			}

			Import.ImportExisting(
				new ImportDatabaseModel
				{
					ConnStringAppDatabase = connString,
					ConnStringAnalyticsDatabase = connStringAnal,
					Tenant = "tenant"
				});

			using (TenantUnitOfWork.Start())
				Tenants.Tenants().Single(x => x.Name == "tenant").RtaKey.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGenerateRtaKeyWith10RandomCharacters()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			TestPolutionCleaner.Clean("tenant1", "appuser1");
			TestPolutionCleaner.Clean("tenant2", "appuser2");

			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant1",
				AppUser = "appuser1",
			});
			Database.CreateDatabases(new CreateTenantModelForTest
			{
				Tenant = "tenant2",
				AppUser = "appuser2",
				FirstUser = "seconduser"
			});

			using (TenantUnitOfWork.Start())
			{
				var key1 = Tenants.Tenants().Single(x => x.Name == "tenant1").RtaKey;
				var key2 = Tenants.Tenants().Single(x => x.Name == "tenant2").RtaKey;
				key1.Should().Match(new Regex("[A-Za-z0-9]{10}"));
				key2.Should().Match(new Regex("[A-Za-z0-9]{10}"));
				key1.Should().Not.Be.EqualTo(key2);
			}
		}
	}
}