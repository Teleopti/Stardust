﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;

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
		private SqlVersion _sqlVersion;
		private DatabaseFolder _databaseFolder;
		private DatabaseVersionInformation _databaseVersionInformation;
		private SchemaVersionInformation _schemaVersionInformation;
		private ConfigureSystem _configureSystem;
		private DatabaseRestorer _restorer;

		private readonly RepositoryRootFolder _repositoryRootFolder = new RepositoryRootFolder();

		public ExecuteSql MasterExecuteSql() => _masterExecuteSql ?? (_masterExecuteSql = new ExecuteSql(() => connectAndOpen(connectionStringToMaster()), _log));
		public ExecuteSql ExecuteSql() => _executeSql ?? (_executeSql = new ExecuteSql(() => connectAndOpen(connectionString()), _log));
		public SqlVersion SqlVersion() => _sqlVersion ?? (_sqlVersion = new ServerVersionHelper(_command.CreateDatabase ||
																								_command.RecreateDatabaseIfNotExistsOrNewer ||
																								!string.IsNullOrEmpty(_command.RestoreBackup) ||
																								!string.IsNullOrEmpty(_command.RestoreBackupIfNotExistsOrNewer) ? MasterExecuteSql() : ExecuteSql()).Version());

		public DatabaseFolder DatabaseFolder() => _databaseFolder ?? (_databaseFolder = new DatabaseFolder(new DbManagerFolder(_command.DbManagerFolderPath)));
		public DatabaseVersionInformation DatabaseVersionInformation() => _databaseVersionInformation ?? (_databaseVersionInformation = new DatabaseVersionInformation(DatabaseFolder(), ExecuteSql()));
		public SchemaVersionInformation SchemaVersionInformation() => _schemaVersionInformation ?? (_schemaVersionInformation = new SchemaVersionInformation(DatabaseFolder()));
		private ConfigureSystem appRelatedDatabaseTasks() => _configureSystem ?? (_configureSystem = new ConfigureSystem(ExecuteSql()));
		public DatabaseRestorer Restorer() => _restorer ?? (_restorer = new DatabaseRestorer(MasterExecuteSql(), ExecuteSql(), _repositoryRootFolder, new DebugSetupDatabaseFolder(_repositoryRootFolder), appRelatedDatabaseTasks()));

		public bool DatabaseExists()
		{
			try
			{
				ExecuteSql().Execute(c => { });
				return true;
			}
			catch(Exception e)
			{
				_log.Write($"An error occurred: {e}\n{e.StackTrace}", "WARN");
				return false;
			}
		}

		public bool LoginExist()
		{
			if (!_command.IsWindowsGroupName)
			{
				try
				{
					using (connectAndOpen(connectionStringAppLogOn(SqlVersion().IsAzure ? _command.DatabaseName : DatabaseHelper.MasterDatabaseName)))
					{
					}

					return true;
				}
				catch
				{
					return false;
				}
			}

			if (!SqlVersion().IsAzure)
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