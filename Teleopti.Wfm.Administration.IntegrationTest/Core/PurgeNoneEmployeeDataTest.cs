using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using NHibernate;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Controllers;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.IntegrationTest.ControllerActions;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest.Core
{
	[WfmAdminTest]
	[AllTogglesOn]
	public class PurgeNoneEmployeeDataTest 
	{ 
		public IPersonInfoPersister PersonInfoPersister;
		public IPurgeNoneEmployeeData Target;
		public ImportController ImportController;
		public MutableNow Now;
		public IHashFunction HashFunction;
		public TestPollutionCleaner TestPollutionCleaner;
		public IDatabaseHelperWrapper DatabaseHelperWrapper;
		public ILoadAllTenants LoadAllTenants;
		public ITenantUnitOfWork TenantUnitOfWork;
		public ICheckPasswordStrength CheckPasswordStrength;

		[Test]
		public void ShouldPurge()
		{
			Now.Is(new DateTime(2018,04,1));
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			//Only done once
			TestPollutionCleaner.Clean("tenant", "appuser");
			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "password";
			
			var sqlVersion = new SqlVersion(12);
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "password");
			DatabaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCC7, "appuser", "password", sqlVersion,
				"NewFineTenant", 1);

			var builderAnal = TestPollutionCleaner.TestTenantAnalyticsConnection();
			builderAnal.IntegratedSecurity = false;
			builderAnal.UserID = "dbcreatorperson";
			builderAnal.Password = "password";
			
			DatabaseHelperWrapper.CreateDatabase(builderAnal.ConnectionString, DatabaseType.TeleoptiAnalytics, "appuser", "password", sqlVersion, "NewFineTenant", 1);

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
				AppDatabase = TestPollutionCleaner.TestTenantConnection().InitialCatalog,
				AnalyticsDatabase = TestPollutionCleaner.TestTenantAnalyticsConnection().InitialCatalog,
				Tenant = "NewFineTenant"
			};

			ImportController.ImportExisting(importModel);

			var uow = TenantUnitOfWork as TenantUnitOfWorkManager;
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var allTenants = LoadAllTenants.Tenants();
				allTenants.ForEach(createPersons);
				
				updateSetting(allTenants.First().DataSourceConfiguration.ApplicationConnectionString, 1);
				updateSetting(allTenants.Last().DataSourceConfiguration.ApplicationConnectionString, 2);
			}

			Target.Purge();

			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var personInfos = loadPersonFromTenant(uow.CurrentSession());
				personInfos.Count.Should().Be.EqualTo(9);
			}
		}

		[Test]
		public void ShouldPurgeWithIncorrectSetting()
		{
			Now.Is(new DateTime(2018, 04, 1));
			DataSourceHelper.CreateDatabasesAndDataSource(DataSourceHelper.MakeLegacyWay());
			var builder = TestPollutionCleaner.TestTenantConnection();
			builder.IntegratedSecurity = false;
			builder.UserID = "dbcreatorperson";
			builder.Password = "password";

			var sqlVersion = new SqlVersion(12);
			DatabaseHelperWrapper.CreateLogin(builder.ConnectionString, "appuser", "password");
			DatabaseHelperWrapper.CreateDatabase(builder.ConnectionString, DatabaseType.TeleoptiCCC7, "appuser", "password", sqlVersion,
				"NewFineTenant", 1);

			var builderAnal = TestPollutionCleaner.TestTenantAnalyticsConnection();
			builderAnal.IntegratedSecurity = false;
			builderAnal.UserID = "dbcreatorperson";
			builderAnal.Password = "password";

			DatabaseHelperWrapper.CreateDatabase(builderAnal.ConnectionString, DatabaseType.TeleoptiAnalytics, "appuser", "password", sqlVersion, "NewFineTenant", 1);

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
				AppDatabase = TestPollutionCleaner.TestTenantConnection().InitialCatalog,
				AnalyticsDatabase = TestPollutionCleaner.TestTenantAnalyticsConnection().InitialCatalog,
				Tenant = "NewFineTenant"
			};

			ImportController.ImportExisting(importModel);

			var uow = TenantUnitOfWork as TenantUnitOfWorkManager;
			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var allTenants = LoadAllTenants.Tenants();
				allTenants.ForEach(createPersons);

				//no settings
				using (var connection = new SqlConnection(allTenants.First().DataSourceConfiguration.ApplicationConnectionString))
				{
					connection.Open();
					using (var updateCommand = new SqlCommand(
						@"delete from PurgeSetting where [key] = 'MonthsToKeepPersonalData'", connection))
					{
						updateCommand.ExecuteNonQuery();
					}
				}

				updateSetting(allTenants.Last().DataSourceConfiguration.ApplicationConnectionString, 2);
			}

			Target.Purge();

			using (uow.EnsureUnitOfWorkIsStarted())
			{
				var personInfos = loadPersonFromTenant(uow.CurrentSession());
				personInfos.Count.Should().Be.EqualTo(11);
			}


		}


		private IList<string> loadPersonFromTenant(ISession currentTenantSession)
		{
			return currentTenantSession.CreateSQLQuery(
					"select ApplicationLogonName from [Tenant].[PersonInfo]")
				.List<string>();
			
		}

		private void updateSetting(string connString, int monthsToKeepPersonalData)
		{
			using (var connection = new SqlConnection(connString))
			{
				connection.Open();
				using (var updateCommand = new SqlCommand(
					@"update PurgeSetting set [value] = @month where [key] = 'MonthsToKeepPersonalData'",connection))
				{
					updateCommand.Parameters.AddWithValue("@month", monthsToKeepPersonalData);
					updateCommand.ExecuteNonQuery();
				}
			}
		}

		private void createPersons(Tenant tenant)
		{
			using (var connection = new SqlConnection(tenant.DataSourceConfiguration.ApplicationConnectionString))
			{
				connection.Open();
				var personId = Guid.NewGuid();

				createPersonInTenant(connection, personId, null);
				persistPersonInfo(tenant, personId, 84);

				personId = Guid.NewGuid();
				createPersonInTenant(connection, personId, Now.UtcDateTime().AddMonths(1));
				persistPersonInfo(tenant, personId, 85);

				foreach (var i in Enumerable.Range(0,5))
				{
					personId = Guid.NewGuid();

					createPersonInTenant(connection, personId, Now.UtcDateTime().AddMonths(-i));
					persistPersonInfo(tenant,personId,i);

				}
			}
		}

		private void createPersonInTenant(SqlConnection connection, Guid personId, DateTime? terminalDate)
		{
			using (var insertCommand = new SqlCommand(
				@"INSERT INTO [dbo].[Person]([Id],[Version] ,[UpdatedBy] ,[UpdatedOn] ,[Email] ,[Note]
							   ,[EmploymentNumber],[FirstName] ,[LastName] ,[DefaultTimeZone] ,[IsDeleted],[FirstDayOfWeek],[TerminalDate])    		VALUES
           (@id,@version,@updatedby,@updateon,@email,@note,@emp,@firstname,@lastname,@defaultTimezone,@isdeleted,@fdow,@td)",
				connection))
			{
				insertCommand.Parameters.AddWithValue("@id", personId);
				insertCommand.Parameters.AddWithValue("@version", 1);
				insertCommand.Parameters.AddWithValue("@updatedby", new Guid("3F0886AB-7B25-4E95-856A-0D726EDC2A67"));
				insertCommand.Parameters.AddWithValue("@updateon", Now.UtcDateTime());
				insertCommand.Parameters.AddWithValue("@email", " ");
				insertCommand.Parameters.AddWithValue("@note", " ");
				insertCommand.Parameters.AddWithValue("@emp", " ");
				insertCommand.Parameters.AddWithValue("@firstname", " ");
				insertCommand.Parameters.AddWithValue("@lastname", " ");
				insertCommand.Parameters.AddWithValue("@defaultTimezone", "UTC");
				insertCommand.Parameters.AddWithValue("@isdeleted", 0);
				insertCommand.Parameters.AddWithValue("@fdow", 1);
				if(terminalDate.HasValue)
					insertCommand.Parameters.AddWithValue("@td", terminalDate.GetValueOrDefault());
				else
					insertCommand.Parameters.AddWithValue("@td", DBNull.Value);
				//set lock time to null
				insertCommand.ExecuteNonQuery();
			}
		}

		private void persistPersonInfo(Tenant tenant,Guid personId, int counter)
		{
			var personInfo = new PersonInfo(tenant, personId);
			personInfo.SetIdentity(tenant.Name + "Identity" + counter);
			personInfo.SetApplicationLogonCredentials(CheckPasswordStrength, tenant.Name + "AppLogon" + counter, "jnlai65!jsdnfk", HashFunction);
			PersonInfoPersister.Persist(personInfo);
		}
		
	}

	
}