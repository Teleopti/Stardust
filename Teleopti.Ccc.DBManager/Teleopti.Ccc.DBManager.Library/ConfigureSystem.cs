using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Support.Library;

namespace Teleopti.Ccc.DBManager.Library
{
	public class ConfigureSystem
	{
		private readonly ExecuteSql _execute;

		public ConfigureSystem(ExecuteSql execute)
		{
			_execute = execute;
		}

		public void AddSystemUser(Guid personId, string firstName, string lastName)
		{
			var sql = string.Format(@"INSERT INTO Person 
(Id, [Version], UpdatedBy, UpdatedOn, Email, Note, EmploymentNumber,FirstName, LastName, DefaultTimeZone,IsDeleted,FirstDayOfWeek)
VALUES('{2}', 1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67',  GETUTCDATE(), '', '', '', '{0}', '{1}', 'UTC', 0, 1)
INSERT INTO PersonInApplicationRole
SELECT '{2}', '193AD35C-7735-44D7-AC0C-B8EDA0011E5F' , GETUTCDATE()", firstName, lastName, personId);
			_execute.ExecuteNonQuery(sql);
		}

		//
		public void AddSystemUserToPersonInfo(Guid personId, string userName, string password, string tenantPassword)
		{
			var sql = string.Format(@"if exists (select top 1 id from Tenant.PersonInfo WHERE ApplicationLogonName = '{0}') return
if (select top 1 id from Tenant.Tenant) = 1
BEGIN INSERT INTO Tenant.PersonInfo 
(Id, Tenant, [Identity], ApplicationLogonName, ApplicationLogonPassword, LastPasswordChange, InvalidAttemptsSequenceStart, IsLocked, InvalidAttempts, TenantPassword)
VALUES('{2}', 1, null, '{0}', '{1}', GETUTCDATE(), GETUTCDATE(), 0, 0,  '{3}')
END", userName, password, personId, tenantPassword);
			_execute.ExecuteNonQuery(sql);
		}

		public void CleanByAnalyticsProcedure()
		{
			_execute.ExecuteNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1", 60);
		}

		public bool IsCorrectDb(DatabaseType databaseType)
		{
			string sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') ";
			if (databaseType.Equals(DatabaseType.TeleoptiAnalytics))
				sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[mart].[dim_person]') ";
			if (databaseType.Equals(DatabaseType.TeleoptiCCCAgg))
				sql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[log_object]') ";

			return Convert.ToBoolean(_execute.ExecuteScalar(sql));
		}

		public void MergePersonAssignments()
		{
			_execute.ExecuteNonQuery("exec [dbo].[MergePersonAssignments]");
		}

		public void SetTenantConnectionInfo(string name, string appConnectionString, string analyticsConnectionString)
		{
			_execute.ExecuteNonQuery(
				"update tenant.tenant set active = 1, name = @name, applicationconnectionstring = @app, analyticsconnectionstring = @analytics",
				parameters:
				new Dictionary<string, object>
				{
					{"@name", name ?? string.Empty},
					{"@app", appConnectionString},
					{"@analytics", analyticsConnectionString}
				});
		}

		public void ActivateAllTenants()
		{
			_execute.ExecuteNonQuery("update tenant.tenant set active = 1");
		}

		public void PersistAuditSetting()
		{
			_execute.ExecuteNonQuery("exec Auditing.InitAuditTables");
			_execute.ExecuteNonQuery("delete from auditing.Auditsetting");
			_execute.ExecuteNonQuery("insert into auditing.Auditsetting (id, IsScheduleEnabled) values (" +
									 AuditSetting.TheId + ", 1)");
		}

		public void TryAddTenantAdminUser()
		{
			_execute.ExecuteNonQuery(@" if not exists (select * from Tenant.AdminUser )
										INSERT INTO Tenant.AdminUser(Name, Email, Password)
										VALUES('FirstAdmin', 'first@admin.is', '###70D74A6BBA33B5972EADAD9DD449D273E1F4961D###')"); // password=demo
		}

		public void ConfigureSalesDemoDatabaseUserAsMe()
		{
			var userIdInSalesDemo = "10957ad5-5489-48e0-959a-9b5e015b2b5c";
			_execute.ExecuteNonQuery($@"
				UPDATE Tenant.PersonInfo
				SET [Identity] = '{Environment.UserDomainName}\{Environment.UserName}'
				WHERE Id = '{userIdInSalesDemo}'
				");
		}
	}
}