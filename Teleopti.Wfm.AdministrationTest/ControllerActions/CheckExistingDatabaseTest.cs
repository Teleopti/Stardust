using System;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest.ControllerActions
{
	[TenantTest]
	public class CheckExistingDatabaseTest
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

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
			TenantUnitOfWork.Start();

			var connStringBuilder = new SqlConnectionStringBuilder(CurrentTenantSession.CurrentSession().Connection.ConnectionString)
			{
				InitialCatalog = dummy
			};
			string connString = connStringBuilder.ConnectionString;
			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			if (!helper.Exists())
			{
				helper.CreateByDbManager();
				helper.CreateSchemaByDbManager();
			}
			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = RandomName.Make() };
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();
			TenantUnitOfWork.CommitAndDisposeCurrent();
		}

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotExists()
		{
			TenantUnitOfWork.Start();

			var connStringBuilder = new SqlConnectionStringBuilder(CurrentTenantSession.CurrentSession().Connection.ConnectionString)
			{
				InitialCatalog = Guid.NewGuid().ToString()
			};
			//correct format but does not exist
			string connString = connStringBuilder.ConnectionString;
			

			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = RandomName.Make() };
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();

			TenantUnitOfWork.CommitAndDisposeCurrent();
		}

		[Test]
		public void ShouldReturnSuccessTrueWhenDatabaseExists()
		{
			TenantUnitOfWork.Start();

			var connStringBuilder = new SqlConnectionStringBuilder( CurrentTenantSession.CurrentSession().Connection.ConnectionString)
			{
				InitialCatalog = dummy
			};
			string connString = connStringBuilder.ConnectionString;
			connStringBuilder.InitialCatalog = dummyAnal;
			string connStringAnal = connStringBuilder.ConnectionString;

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
			var importModel = new ImportDatabaseModel { ConnStringAppDatabase = connString, ConnStringAnalyticsDatabase = connStringAnal };
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.True();

			TenantUnitOfWork.CommitAndDisposeCurrent();

			

		}


	}
}