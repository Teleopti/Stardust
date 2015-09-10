using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
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
	}
}