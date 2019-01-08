using System;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Support.Library;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.IntegrationTest
{
	public class DatabaseHelperWrapperFake : IDatabaseHelperWrapper
	{
		public DbCheckResultModel Exists(string databaseConnectionString, DatabaseType databaseType)
		{
			return new DbCheckResultModel {Exists = true};
		}

		public void CreateDatabase(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion, string tenant, int tenantId)
		{
		}

		public void AddSystemUser(string connectionToNewDb, Guid personId, string firstName, string lastName)
		{
		}

		public bool LoginExists(string connectionToNewDb, string login, SqlVersion sqlVersion)
		{
			return false;
		}

		public void CreateLogin(string connectionToNewDb, string login, string password)
		{
		}

		public bool HasCreateDbPermission(string connectionString)
		{
			return true;
		}

		public bool HasCreateViewAndLoginPermission(string connectionString)
		{
			return true;
		}

		public bool LoginCanBeCreated(string connectionString, string login, string password, out string message)
		{
			message = "";
			return true;
		}

		public void AddDatabaseUser(string connectionToNewDb, DatabaseType databaseType, string login, string pwd, SqlVersion sqlVersion)
		{
		}

		public SqlVersion Version(string connectionToNewDb)
		{
			return new SqlVersion(12);
		}

		public void AddSystemUserToPersonInfo(string connectionToNewDb, Guid personId, string userName, string password,
			string tenantPassword)
		{
			
		}

		public void DeActivateTenantOnImport(string connectionString)
		{
			throw new NotImplementedException();
		}

		public void ActivateTenantOnDelete(Tenant tenant)
		{
			throw new NotImplementedException();
		}
	}
}