using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using Teleopti.Ccc.Domain.Azure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Ccc.DBManager.Library
{
	public class PermissionsHelper
	{
		private static readonly Version SQL2005SP2 = Version.Parse("9.00.3042"); //http://www.sqlteam.com/article/sql-server-versions

		private readonly IUpgradeLog _logger;
		private readonly DatabaseFolder _folder;
		private readonly ExecuteSql _executeSql;

		public PermissionsHelper(IUpgradeLog logger, DatabaseFolder folder, ExecuteSql executeSql)
		{
			_logger = logger;
			_folder = folder;
			_executeSql = executeSql;
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreatePermissions(string user, string pwd)
		{
			//if application login = sa then don't bother to do anything
			if (compareStringLowerCase(user, @"sa"))
				return;

			if (!AzureCommon.IsAzure)
			{
				//Create or Re-link e.g Alter the DB-user from SQL-Login
				var createDBUser = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] FOR LOGIN [{0}]", user);

				string relinkSqlUser;
				if (sqlVersionGreaterThen(SQL2005SP2))
					relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"ALTER USER [{0}] WITH LOGIN = [{0}]", user);
				else
					relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"sp_change_users_login 'Update_One', '{0}', '{0}'", user);

				if (dbUserExist(user))
				{
					_logger.Write("DB user already exist, re-link ...");
					_executeSql.ExecuteTransactionlessNonQuery(relinkSqlUser);
				}
				else
				{
					_logger.Write("DB user is missing. Create DB user ...");
					_executeSql.ExecuteTransactionlessNonQuery(createDBUser);
				}
			}
			else
			{
				var checkExistsSpecificForAzure = string.Format(CultureInfo.CurrentCulture, @"SELECT COUNT(*) FROM sys.database_principals WHERE name = '{0}'", user);
				if (_executeSql.ExecuteScalar(checkExistsSpecificForAzure) == 0)
				{
					var createDBUser = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] WITH PASSWORD='{1}'", user, pwd);
					_logger.Write("DB user is missing. Creating contained DB user ...");
					_executeSql.ExecuteTransactionlessNonQuery(createDBUser);
				}
				//var permSql = string.Format(CultureInfo.CurrentCulture, "ALTER AUTHORIZATION ON SCHEMA::[db_owner] TO[{0}]", user);
				//_executeSql.ExecuteTransactionlessNonQuery(permSql);
				//permSql = string.Format(CultureInfo.CurrentCulture, "ALTER ROLE[db_owner] ADD MEMBER[{0}]", user);
				//_executeSql.ExecuteTransactionlessNonQuery(permSql);

				// fix for PBI 45977 revoke dbowner permissions wrongly added before
				var permSql = string.Format(CultureInfo.CurrentCulture, "ALTER AUTHORIZATION ON SCHEMA::[db_owner] TO[dbo]", user);
				_executeSql.ExecuteTransactionlessNonQuery(permSql);
				permSql = string.Format(CultureInfo.CurrentCulture, "ALTER ROLE[db_owner] DROP MEMBER[{0}]", user);
				_executeSql.ExecuteTransactionlessNonQuery(permSql);
			}
			//Add permission
			var fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", _folder.Path());

			var sql = File.ReadAllText(fileName);

			sql = sql.Replace("$(LOGIN)", user);

			_executeSql.ExecuteNonQuery(sql);

			_logger.Write("Created Permissions!");
		}

		private bool dbUserExist(string sqlLogin)
		{
			const string sql = @"select count(*) from sys.sysusers where name = @SQLLogin";
			return Convert.ToBoolean(_executeSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlLogin } }));
		}

		private bool compareStringLowerCase(string stringA, string stringB)
		{
			return string.Compare(stringA, stringB, true, CultureInfo.CurrentCulture) == 0;
		}

		[SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private bool sqlVersionGreaterThen(Version checkVersion)
		{
			var serverVersion = new Version();
			_executeSql.Execute(sqlConnection => { serverVersion = Version.Parse(sqlConnection.ServerVersion); });

			return serverVersion >= checkVersion;
		}
	}
}