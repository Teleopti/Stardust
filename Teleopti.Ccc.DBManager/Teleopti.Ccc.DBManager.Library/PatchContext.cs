using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class PatchContext
	{
		private readonly PatchCommand _command;
		private readonly IUpgradeLog _log;
		private const string applicationName = "Teleopti.Ccc.DBManager";
		private const string currentLanguage = "us_english";

		public PatchContext(PatchCommand command, IUpgradeLog log)
		{
			_command = command;
			_log = log;
		}

		private ExecuteSql _masterExecuteSql;
		private ExecuteSql _executeSql;

		public ExecuteSql MasterExecuteSql() =>
			_masterExecuteSql ?? (_masterExecuteSql = new ExecuteSql(() => connectAndOpen(connectionStringToMaster()), _log));

		public ExecuteSql ExecuteSql() =>
			_executeSql ?? (_executeSql = new ExecuteSql(() => connectAndOpen(connectionString()), _log));

		public bool LoginExist()
		{
			if (!_command.IsWindowsGroupName)
			{
				try
				{
					using (connectAndOpen(connectionStringAppLogOn(_command.IsAzure ? _command.DatabaseName : DatabaseHelper.MasterDatabaseName)))
					{
					}

					return true;
				}
				catch
				{
					return false;
				}
			}

			if (!_command.IsAzure)
			{
				return verifyWinGroup(_command.AppUserName);
			}

			return false;
		}

		private string connectionString()
		{
			var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
			{
				DataSource = _command.ServerName,
				InitialCatalog = _command.DatabaseName,
				ApplicationName = applicationName,
				CurrentLanguage = currentLanguage
			};
			if (_command.UseIntegratedSecurity)
				sqlConnectionStringBuilder.IntegratedSecurity = _command.UseIntegratedSecurity;
			else
			{
				sqlConnectionStringBuilder.UserID = _command.UserName;
				sqlConnectionStringBuilder.Password = _command.Password;
			}

			return sqlConnectionStringBuilder.ConnectionString;
		}

		private string connectionStringToMaster()
		{
			var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
			{
				DataSource = _command.ServerName,
				InitialCatalog = DatabaseHelper.MasterDatabaseName,
				CurrentLanguage = currentLanguage,
				ApplicationName = applicationName
			};
			if (_command.UseIntegratedSecurity)
				sqlConnectionStringBuilder.IntegratedSecurity = _command.UseIntegratedSecurity;
			else
			{
				sqlConnectionStringBuilder.UserID = _command.UserName;
				sqlConnectionStringBuilder.Password = _command.Password;
			}

			return sqlConnectionStringBuilder.ConnectionString;
		}

		private string connectionStringAppLogOn(string databaseName)
		{
			var sqlConnectionStringBuilder = new SqlConnectionStringBuilder
			{
				DataSource = _command.ServerName,
				InitialCatalog = databaseName,
				CurrentLanguage = currentLanguage,
				ApplicationName = applicationName
			};
			if (_command.UseIntegratedSecurity)
				sqlConnectionStringBuilder.IntegratedSecurity = _command.UseIntegratedSecurity;
			else
			{
				sqlConnectionStringBuilder.UserID = _command.AppUserName;
				sqlConnectionStringBuilder.Password = _command.AppUserPassword;
			}

			return sqlConnectionStringBuilder.ConnectionString;
		}

		private bool verifyWinGroup(string winNtGroup)
		{
			const string sql = @"SELECT count(name) from sys.syslogins where isntgroup = 1 and name = @WinNTGroup";
			return Convert.ToBoolean(MasterExecuteSql().ExecuteScalar(sql, parameters: new Dictionary<string, object> {{"@WinNTGroup", winNtGroup}}));
		}

		private SqlConnection connectAndOpen(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			sqlConnection.InfoMessage += sqlConnectionInfoMessage;
			return sqlConnection;
		}

		private void sqlConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			_log.Write(e.Message);
		}

		public bool VersionTableExists()
		{
			const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'DatabaseVersion'";
			return Convert.ToBoolean(ExecuteSql().ExecuteScalar(sql));
		}

		public bool IsDbOwner()
		{
			const string sql = "select IS_MEMBER ('db_owner')";
			return Convert.ToBoolean(ExecuteSql().ExecuteScalar(sql));
		}

		public bool HasSrvSecurityAdmin()
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='ALTER ANY LOGIN'";
			return Convert.ToBoolean(MasterExecuteSql().ExecuteScalar(sql));
		}

		public bool HasSrvDbCreator()
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='CREATE ANY DATABASE'";
			return Convert.ToBoolean(MasterExecuteSql().ExecuteScalar(sql));
		}
	}
}