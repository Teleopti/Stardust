using System;
using System.Configuration;
using System.Data.SqlClient;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
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
	public class CreateDatabasesTest
	{
		public DatabaseController Target;
		public DatabaseHelperWrapper DatabaseHelperWrapper;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICurrentTenantSession CurrentTenantSession;

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

            DatabaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString,"nodbcreator", "password");

			var model = new CreateTenantModel { Tenant = "New Tenant", CreateDbUser = "nodbcreator", CreateDbPassword = "password", };

			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.False();
			result.Message.Should().Be.EqualTo("The user does not have permission to create database.");
		}

		[Test]
		public void ShouldReturnTrueCreatedDb()
		{
			DataSourceHelper.CreateDataSource(new NoMessageSenders(), "TestData");
			var connStringBuilder =
				new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);

			DatabaseHelperWrapper.CreateLogin(connStringBuilder.ConnectionString, "dbcreatorperson", "password");
			connStringBuilder.InitialCatalog = "master";
			using (var conn = new SqlConnection(connStringBuilder.ConnectionString))
			{
				conn.Open();
				using (var cmd = conn.CreateCommand())
				{
					cmd.CommandText = string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'dbcreator'", "dbcreatorperson");
					cmd.ExecuteNonQuery();
					cmd.CommandText = string.Format("EXEC sp_addsrvrolemember @loginame= '{0}', @rolename = 'securityadmin'", "dbcreatorperson");
					cmd.ExecuteNonQuery();
				}
			}

			var model = new CreateTenantModel
			{
				Tenant = "New Tenant",
				CreateDbUser = "dbcreatorperson",
				CreateDbPassword = "password",
				AppUser = "new TenantAppUser",
				AppPassword = "NewTenantAppPassword",
				FirstUser = "Thefirstone",
				FirstUserPassword = "Agood@pasw0rd",
				BusinessUnit = "My First BU"
			};

			var result = Target.CreateDatabases(model).Content;
			result.Success.Should().Be.True();
			
		}
	}
}