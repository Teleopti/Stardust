using System;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public interface IDatabaseHelperWrapper
	{
		DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType);
		void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string login, SqlVersion sqlVersion, string tenant, int tenantId);
		void AddSuperUser(string connectionToNewDb, Guid personId, string firstName, string lastName);
		void AddBusinessUnit(string connectionToNewDb, string name);
		bool LoginExists(string connectionToNewDb, string login, SqlVersion sqlVersion);
		void CreateLogin(string connectionToNewDb, string login, string password, SqlVersion sqlVersion);
		bool HasCreateDbPermission(string connectionString, SqlVersion sqlVersion);
		bool HasCreateViewAndLoginPermission(string connectionString, SqlVersion sqlVersion);
		bool LoginCanBeCreated(string connectionString, string login, string password, SqlVersion sqlVersion, out string message);
		void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login, SqlVersion sqlVersion);
		SqlVersion Version(string connectionToNewDb);
	}
}