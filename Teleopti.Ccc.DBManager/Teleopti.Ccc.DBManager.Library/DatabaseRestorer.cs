using System.IO;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DebugSetupDatabaseFolder
	{
		private readonly RepoFolder _repoFolder;

		public DebugSetupDatabaseFolder(RepoFolder repoFolder)
		{
			_repoFolder = repoFolder;
		}

		public string Path()
		{
			var path = System.IO.Path.Combine(_repoFolder.Path(), @".debug-Setup\database");
			return System.IO.Path.GetFullPath(path);
		}

		public override string ToString() => Path();
	}

	public class RepoFolder
	{
		public string Path() => System.IO.Path.GetFullPath(locateFolderUsingBlackMagic());

		private static string locateFolderUsingBlackMagic()
		{
			if (Directory.Exists(@"..\..\..\..\.debug-Setup"))
				return @"..\..\..\..";
			if (Directory.Exists(@"..\..\..\.debug-Setup"))
				return @"..\..\..";
			if (Directory.Exists(@"..\..\.debug-Setup"))
				return @"..\..";
			if (Directory.Exists(@"..\.debug-Setup"))
				return @"..";
			if (Directory.Exists(@".debug-Setup"))
				return @".";
			return null;
		}

		public override string ToString() => Path();
	}

	public class DatabaseRestorer
	{
		private readonly ExecuteSql _masterSql;
		private readonly ExecuteSql _sql;
		private readonly RepoFolder _repoFolder;
		private readonly DebugSetupDatabaseFolder _debugDbFolder;

		public DatabaseRestorer(ExecuteSql masterSql, ExecuteSql sql, RepoFolder repoFolder, DebugSetupDatabaseFolder debugDbFolder)
		{
			_masterSql = masterSql;
			_sql = sql;
			_repoFolder = repoFolder;
			_debugDbFolder = debugDbFolder;
		}

		public void Restore(PatchCommand command)
		{
			restore(command.DatabaseType, command.DatabaseName, command.RestoreBackup);
			createLoginDropUsers(command.DatabaseName, command.AppUserName, command.AppUserPassword);
			setTenantActive(command.DatabaseType);
		}

		private void restore(DatabaseType databaseType, string databaseName, string bakFile)
		{
			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\RestoreDatabase.sql");
			if (databaseType == DatabaseType.TeleoptiAnalytics)
				file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\RestoreAnalytics.sql");
			var script = File.ReadAllText(file);

			script = replaceVariables(script, bakFile, databaseName, null, null);

			_masterSql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}

		private void createLoginDropUsers(string databaseName, string user, string password)
		{
			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\CreateLoginDropUsers.sql");
			var script = File.ReadAllText(file);

			script = replaceVariables(script, null, databaseName, user, password);

			_sql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}

		private void setTenantActive(DatabaseType databaseType)
		{
			if (databaseType != DatabaseType.TeleoptiCCC7)
				return;

			var file = Path.Combine(_debugDbFolder.Path(), @"tsql\DemoDatabase\SetTenantActive.sql");
			var script = File.ReadAllText(file);

			script = replaceVariables(script, null, null, null, null);

			_sql.ExecuteTransactionlessNonQuery(script, Timeouts.CommandTimeout);
		}

		private string replaceVariables(
			string script,
			string bakFile,
			string databaseName,
			string user,
			string password)
		{
			script = script.Replace(":on error exit", "");
			script = script.Replace("$(SQLLogin)", user);
			script = script.Replace("$(SQLPwd)", password);
			script = script.Replace("$(TELEOPTICCC)", databaseName);
			script = script.Replace("$(TELEOPTIANALYTICS)", databaseName);
			script = script.Replace("$(TELEOPTIAGG)", databaseName);
			script = script.Replace("$(DATABASENAME)", databaseName);
			script = script.Replace("$(BAKFILE)", bakFile);
			script = script.Replace("$(DATAFOLDER)", Path.Combine(_repoFolder.Path(), @".com.teleopti.wfm.developer.tools"));
			return script;
		}

	}
}