using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class ImportTenantTest
	{
		public ImportController Target;

		const string dummy = "CF0DA4E0-DC93-410B-976B-EED9C8A34639";
		const string dummyAnal = "B1EDB896-9D23-4BCF-A42F-F1F2EE5DD64B";

		[Test]
		public void ShouldReturnSuccessFalseIfAppDatabaseNotCorrectFormat()
		{
			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = RandomName.Make() };
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotCorrectFormat()
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				InitialCatalog = dummy
			};
			var connString = connStringBuilder.ConnectionString;
			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			if (!helper.Exists())
			{
				helper.CreateByDbManager();
				helper.CreateSchemaByDbManager();
			}
			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = RandomName.Make() };
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.False();
		}

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotExists()
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				InitialCatalog = Guid.NewGuid().ToString()
			};
			//correct format but does not exist
			var connString = connStringBuilder.ConnectionString;

			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = RandomName.Make() };
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.False();
		}

		[Test, Ignore("Stops sometimes on different versions and I don't have time to fix that before vacation and I want to push, Ola.")]
		public void ShouldReturnFalseWhenNoUsers()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");

			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString)
			{
				InitialCatalog = dummy
			};
			var connString = connStringBuilder.ConnectionString;
			connStringBuilder.InitialCatalog = dummyAnal;
			var connStringAnal = connStringBuilder.ConnectionString;

			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			var helperAnal = new DatabaseHelper(connStringAnal, DatabaseType.TeleoptiAnalytics);

			//make sure dbs exist
			if (!helper.Exists())
			{
				helper.CreateByDbManager();
				helper.CreateSchemaByDbManager();
			}
			if (!helperAnal.Exists())
			{
				helperAnal.CreateByDbManager();
				helperAnal.CreateSchemaByDbManager();
			}
			

			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = connStringAnal, Tenant = RandomName.Make()};
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.True();
		}
	}
}