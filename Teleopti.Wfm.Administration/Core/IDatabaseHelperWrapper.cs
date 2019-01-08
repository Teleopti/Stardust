using System;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IDatabaseHelperWrapper
	{
		DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType);
		void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion, string tenant, int tenantId);
		void AddSystemUser(string connectionToNewDb, Guid personId, string firstName, string lastName);
		bool LoginExists(string connectionToNewDb, string login, SqlVersion sqlVersion);
		void CreateLogin(string connectionToNewDb, string login, string password, SqlVersion isAzure);
		bool HasCreateDbPermission(string connectionString, SqlVersion sqlVersion);
		bool HasCreateViewAndLoginPermission(string connectionString, SqlVersion sqlVersion);
		bool LoginCanBeCreated(string connectionString, string login, string password, SqlVersion sqlVersion, out string message);
		void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion);
		SqlVersion Version(string connectionToNewDb);
		void AddSystemUserToPersonInfo(string connectionToNewDb, Guid personId, string userName, string password, string tenantPassword);
		void DeActivateTenantOnImport(string connectionString);
		void ActivateTenantOnDelete(Tenant tenant);
	}
}