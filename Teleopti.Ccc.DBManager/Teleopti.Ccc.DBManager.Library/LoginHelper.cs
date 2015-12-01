using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class LoginHelper
	{
		private readonly IUpgradeLog _logger;
		private readonly ExecuteSql _masterExecuteSql;
		private readonly ExecuteSql _executeSql;
		private readonly DatabaseFolder _databaseFolder;
		private readonly NameValueCollection _replaceValues = new NameValueCollection();

		public LoginHelper(IUpgradeLog logger, ExecuteSql masterExecuteSql, ExecuteSql executeSql, DatabaseFolder databaseFolder)
		{
			_logger = logger;
			_masterExecuteSql = masterExecuteSql;
			_executeSql = executeSql;
			_databaseFolder = databaseFolder;
		}

		public void EnablePolicyCheck()
		{
			_replaceValues.Add("CHECK_POLICY=OFF", "CHECK_POLICY=ON");
		}

		private bool azureLoginExist(string sqlLogin)
		{
			const string sql = @"select count(*) from sys.sql_logins where name = @SQLLogin";
			return Convert.ToBoolean(_masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlLogin } }));
		}

		private bool azureDatabaseUserExist(string sqlUser)
		{
			const string sql = @"select count(*) from sys.database_principals where name=@SQLLogin AND authentication_type=2";
			return Convert.ToBoolean(_executeSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlUser } }));
		}

		public bool LoginExists(string login, SqlVersion sqlVersion)
		{
			if (sqlVersion.IsAzure)
			{
				if (sqlVersion.ProductVersion < 12)
					return azureLoginExist(login);

				return azureDatabaseUserExist(login);
			}
			return sqlLoginExists(login);
		}

		private bool sqlLoginExists(string login)
		{
			const string sql = "SELECT 1 FROM syslogins WHERE name = @login";
			return
				Convert.ToBoolean(_masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> {{"@login", login}}));
		}

		public void DropLogin(string user, SqlVersion sqlVersion)
		{
			var sql = string.Format("DROP LOGIN [{0}]", user);
			if (sqlVersion.IsAzure && sqlVersion.ProductVersion >= 12)
				sql = string.Format("DROP USER [{0}]", user);
			_masterExecuteSql.ExecuteTransactionlessNonQuery(sql);
		}

		public void CreateLogin(string user, string pwd, Boolean iswingroup, SqlVersion sqlVersion)
		{
			//TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
			string sql;
			if (sqlVersion.IsAzure)
			{
				if (iswingroup)
					_masterExecuteSql.ExecuteNonQuery("PRINT 'Windows Logins cannot be added to Windows Azure for the momement'");
				else
				{
					if (sqlVersion.ProductVersion >= 12)
					{
						if (azureLoginExist(user))
						{
							_masterExecuteSql.ExecuteTransactionlessNonQuery(string.Format("DROP LOGIN [{0}]", user));
						}
						if (azureDatabaseUserExist(user))
							sql = string.Format("ALTER USER [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						else
							sql = string.Format("CREATE USER [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						_executeSql.ExecuteNonQuery(sql);
					}
					else
					{
						if (azureLoginExist(user))
							sql = string.Format("ALTER LOGIN [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						else
							sql = string.Format("CREATE LOGIN [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						_masterExecuteSql.ExecuteTransactionlessNonQuery(sql);
					}
				}
			}
			else
			{
				string fileName;
				if (iswingroup)
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", _databaseFolder.Path());
				else
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\SQL Logins - Create.sql", _databaseFolder.Path());

				sql = File.ReadAllText(fileName);
				sql = sql.Replace("$(SQLLOGIN)", user);
				sql = sql.Replace("$(SQLPWD)", pwd);
				sql = sql.Replace("$(WINLOGIN)", user);

				foreach (var replaceKey in _replaceValues.AllKeys)
				{
					sql = sql.Replace(replaceKey, _replaceValues[replaceKey]);
				}

				_masterExecuteSql.ExecuteTransactionlessNonQuery(sql);
			}

			_logger.Write("Created login!");
		}
	}
}