using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using Teleopti.Interfaces.Infrastructure;

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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreatePermissions(string user, SqlVersion sqlVersion)
		{
			//if appication login = sa then don't bother to do anything
			if (compareStringLowerCase(user, @"sa"))
				return;

			if ((sqlVersion.IsAzure && sqlVersion.ProductVersion < 12) || !sqlVersion.IsAzure)
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private bool sqlVersionGreaterThen(Version checkVersion)
		{
			var serverVersion = new Version();
			_executeSql.Execute(sqlConnection => { serverVersion = Version.Parse(sqlConnection.ServerVersion); });

			return serverVersion >= checkVersion;
		}
	}
}