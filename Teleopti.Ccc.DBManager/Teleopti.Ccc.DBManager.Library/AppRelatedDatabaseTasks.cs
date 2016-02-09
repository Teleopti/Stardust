using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DBManager.Library
{
	public class AppRelatedDatabaseTasks
	{
		private readonly ExecuteSql _execute;

		public AppRelatedDatabaseTasks(ExecuteSql execute)
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

		public void AddBusinessUnit(string name)
		{
			var sql = string.Format(@"INSERT INTO BusinessUnit
SELECT NEWID(),1, '3F0886AB-7B25-4E95-856A-0D726EDC2A67' , GETUTCDATE(), '{0}', null, 0", name);
			_execute.ExecuteNonQuery(sql);
		}

		public void CleanByAnalyticsProcedure()
		{
			_execute.ExecuteNonQuery("EXEC [mart].[etl_data_mart_delete] @DeleteAll=1");
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
			_execute.ExecuteNonQuery("update tenant.tenant set name = @name, applicationconnectionstring = @app, analyticsconnectionstring = @analytics",
				parameters: new Dictionary<string, object> { { "@name", name ?? string.Empty }, { "@app", appConnectionString }, { "@analytics", analyticsConnectionString } });
		}

		public void PersistAuditSetting()
		{
			_execute.ExecuteNonQuery("delete from auditing.Auditsetting");
			_execute.ExecuteNonQuery("insert into auditing.Auditsetting (id, IsScheduleEnabled) values (" + AuditSettingDefault.TheId + ", 0)");
		}
	}
}