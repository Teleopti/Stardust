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
	public class ImportExistingDatabaseTest
	{
		public ImportController Target;
		public ITenantUnitOfWork TenantUnitOfWork;

		const string dummy = "CF0DA4E0-DC93-410B-976B-EED9C8A34639";
		const string dummyAnal = "B1EDB896-9D23-4BCF-A42F-F1F2EE5DD64B";

		private const string connString = "Data Source=.;Integrated Security=SSPI;Initial Catalog=CF0DA4E0-DC93-410B-976B-EED9C8A34639;Application Name=Teleopti.Tenancy";
		private const string connStringAnal = "Data Source=.;Integrated Security=SSPI;Initial Catalog=B1EDB896-9D23-4BCF-A42F-F1F2EE5DD64B;Application Name=Teleopti.Tenancy";

		[Test]
		public void ShouldCheckIfDatabaseExists()
		{
			var helper = new DatabaseHelper(connString, DatabaseType.TeleoptiCCC7);
			var helperAnal = new DatabaseHelper(connStringAnal, DatabaseType.TeleoptiAnalytics);

			TenantUnitOfWork.Start();

			//create dbs
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

			var importModel = new ImportDatabaseModel { AppDatabase = RandomName.Make(), AnalyticsDatabase = dummyAnal };
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();

			importModel = new ImportDatabaseModel { AppDatabase = dummy, AnalyticsDatabase = RandomName.Make() };
			result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();

			importModel = new ImportDatabaseModel { AppDatabase = dummy, AnalyticsDatabase = dummyAnal };
			result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.True();
			
			result.Should().Be.True();
			TenantUnitOfWork.CommitAndDisposeCurrent();

		}


	}
}