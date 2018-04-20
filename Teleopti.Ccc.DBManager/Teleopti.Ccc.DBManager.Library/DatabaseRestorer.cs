﻿using System.IO;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabaseRestorer
	{
		private readonly ExecuteSql _masterSql;
		private readonly ExecuteSql _sql;
		private readonly RepositoryRootFolder _repositoryRootFolder;
		private readonly DebugSetupDatabaseFolder _debugDbFolder;
		private readonly ConfigureSystem _configureSystem;

		public DatabaseRestorer(
			ExecuteSql masterSql,
			ExecuteSql sql,
			RepositoryRootFolder repositoryRootFolder,
			DebugSetupDatabaseFolder debugDbFolder,
			ConfigureSystem configureSystem)
		{
			_masterSql = masterSql;
			_sql = sql;
			_repositoryRootFolder = repositoryRootFolder;
			_debugDbFolder = debugDbFolder;
			_configureSystem = configureSystem;
		}

		public void Restore(PatchCommand command)
		{
			var bakFile = command.RestoreBackup ?? command.RestoreBackupIfNotExistsOrNewer;
			var dataFolder = command.DataFolder ?? new FileInfo(bakFile).Directory.FullName;
			restore(command.DatabaseType, command.DatabaseName, bakFile, dataFolder);
			createLoginDropUsers(command.DatabaseName, command.AppUserName, command.AppUserPassword);
			if (command.DatabaseType == DatabaseType.TeleoptiCCC7)
				_configureSystem.ActivateAllTenants();
			addLicence(command.DatabaseType);
			if (command.DatabaseType == DatabaseType.TeleoptiCCC7)
			{
				_configureSystem.TryAddTenantAdminUser();
				_configureSystem.ConfigureSalesDemoDatabaseUserAsMe();
			}
		}

		private void restore(DatabaseType databaseType, string databaseName, string bakFile, string dataFolder)
		{
			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\RestoreDatabase.sql");
			if (databaseType == DatabaseType.TeleoptiAnalytics)
				file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\RestoreAnalytics.sql");
			var script = File.ReadAllText(file);
			script = replaceVariables(script, bakFile, dataFolder, databaseName, null, null, null);
			_masterSql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}

		private void addLicence(DatabaseType databaseType)
		{
			if (databaseType != DatabaseType.TeleoptiCCC7)
				return;
			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\AddLic.sql");
			var script = File.ReadAllText(file);
			var licenseFile = Path.GetFullPath(Path.Combine(_repositoryRootFolder.Path(), @"LicenseFiles\Teleopti_RD.xml"));
			script = replaceVariables(script, null, null, null, null, null, licenseFile);
			_sql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}


		private void createLoginDropUsers(string databaseName, string user, string password)
		{
			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\CreateLoginDropUsers.sql");
			var script = File.ReadAllText(file);
			script = replaceVariables(script, null, null, databaseName, user, password, null);
			_sql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}

		private string replaceVariables(
			string script,
			string bakFile,
			string dataFolder,
			string databaseName,
			string user,
			string password,
			string licenseFile)
		{
			script = script.Replace(":on error exit", "");
			script = script.Replace("$(SQLLogin)", user);
			script = script.Replace("$(SQLPwd)", password);
			script = script.Replace("$(TELEOPTICCC)", databaseName);
			script = script.Replace("$(TELEOPTIANALYTICS)", databaseName);
			script = script.Replace("$(TELEOPTIAGG)", databaseName);
			script = script.Replace("$(DATABASENAME)", databaseName);
			script = script.Replace("$(BAKFILE)", bakFile);
			script = script.Replace("$(LicFile)", licenseFile);
			script = script.Replace("$(DATAFOLDER)", dataFolder);
			return script;
		}
	}
}