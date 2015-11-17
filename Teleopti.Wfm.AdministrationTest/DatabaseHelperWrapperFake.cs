using System;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.AdministrationTest
{
	public class DatabaseHelperWrapperFake : IDatabaseHelperWrapper
	{
		public DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType)
		{
			return new DbCheckResultModel {Exists = true};
		}

		public void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string dbPath, string login, SqlVersion sqlVersion, string tenant)
		{
		}

		public void AddSuperUser(string connectionToNewDb, Guid personId, string firstName, string lastName)
		{
		}

		public void AddBusinessUnit(string connectionToNewDb, string name)
		{
		}

		public bool LoginExists(string connectionToNewDb, string login, SqlVersion sqlVersion)
		{
			return false;
		}

		public void CreateLogin(string connectionToNewDb, string login, string password, SqlVersion sqlVersion)
		{
		}

		public bool HasCreateDbPermission(string connectionString, SqlVersion sqlVersion)
		{
			return true;
		}

		public bool HasCreateViewAndLoginPermission(string connectionString, SqlVersion sqlVersion)
		{
			return true;
		}

		public bool LoginCanBeCreated(string connectionString, string login, string password, SqlVersion sqlVersion, out string message)
		{
			message = "";
			return true;
		}

		public void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login)
		{
		}

		public SqlVersion Version(string connectionToNewDb)
		{
			return new SqlVersion(12,false);
		}
	}
}