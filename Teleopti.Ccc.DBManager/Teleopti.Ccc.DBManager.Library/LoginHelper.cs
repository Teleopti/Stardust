using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using Teleopti.Ccc.Domain.Azure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Ccc.DBManager.Library
{
	public class LoginHelper
	{
		private readonly IUpgradeLog _logger;
		private readonly ExecuteSql _masterExecuteSql;
		private readonly DatabaseFolder _databaseFolder;
		private readonly NameValueCollection _replaceValues = new NameValueCollection();

		public LoginHelper(IUpgradeLog logger, ExecuteSql masterExecuteSql, DatabaseFolder databaseFolder)
		{
			_logger = logger;
			_masterExecuteSql = masterExecuteSql;
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

		public bool LoginExists(string login, SqlVersion sqlVersion)
		{
			if (AzureCommon.IsAzure)
			{
				if (sqlVersion.ProductVersion < 12)
					return azureLoginExist(login);
				//should test contained but we can't test that before we create the database
				//return azureDatabaseUserExist(login);
				return false;
			}
			return sqlLoginExists(login);
		}

		private bool sqlLoginExists(string login)
		{
			const string sql = "SELECT 1 FROM syslogins WHERE name = @login";
			return
				Convert.ToBoolean(_masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> {{"@login", login}}));
		}
		
		public void DropLogin(string user)
		{
			if (!AzureCommon.IsAzure)
			{
				var sql = string.Format("DROP LOGIN [{0}]", user);
				_masterExecuteSql.ExecuteTransactionlessNonQuery(sql);	
			}
		}

		public void CreateLogin(string user, string pwd, bool iswingroup)
		{
			//TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
			string sql;
			if (AzureCommon.IsAzure)
			{
				if (iswingroup)
					_masterExecuteSql.ExecuteNonQuery("PRINT 'Windows Logins cannot be added to Windows Azure for the momement'");
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