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
		public TestPolutionCleaner TestPolutionCleaner;

		[Test]
		public void ShouldReturnSuccessFalseIfNotAllPropertiesAreSet()
		{
			var importModel = new ImportDatabaseModel { AppDatabase = RandomName.Make(), AnalyticsDatabase = RandomName.Make(), Server = RandomName.Make()};
			bool result = Target.ImportExisting(importModel).Content.Success;
			result.Should().Be.False();
		}

		[Test]
		public void ShouldReturnSuccessFalseIfAnalDatabaseNotExists()
		{
			var connStringBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			
			var helper = new DatabaseHelper(TestPolutionCleaner.TestTenantConnectionString(), DatabaseType.TeleoptiCCC7);
			var helperAnal = new DatabaseHelper(TestPolutionCleaner.TestTenantAnalyticsConnectionString(), DatabaseType.TeleoptiAnalytics);
				AppDatabase = connStringBuilder.InitialCatalog,
				Server = connStringBuilder.DataSource,
				UserName = connStringBuilder.UserID,
				Password = connStringBuilder.Password,
				AnalyticsDatabase = RandomName.Make()

			var importModel = new ImportDatabaseModel
			{
				ConnStringAppDatabase = TestPolutionCleaner.TestTenantConnectionString(),
				ConnStringAnalyticsDatabase = TestPolutionCleaner.TestTenantAnalyticsConnectionString(),
				Tenant = RandomName.Make()
			};
			Target.ImportExisting(importModel).Content.Success
				.Should().Be.False();
		}
	}
}