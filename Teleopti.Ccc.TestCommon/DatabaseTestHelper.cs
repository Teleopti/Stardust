using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Support.Library;
using Teleopti.Support.Library.Folders;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.TestCommon
{
	public class DatabaseTestHelper
	{
		private readonly string _repositoryRoot;

		public DatabaseTestHelper()
		{
			_repositoryRoot = RepositoryRootFolderLocator.LocateFolderUsingBlackMagic(TestContext.CurrentContext.TestDirectory);
		}

		public void CreateDatabases(string tenant)
		{
			createOrRestoreApplication(tenant);
			createOrRestoreAnalytics();
			createOrRestoreAgg();
		}

		public void BackupApplicationDatabase(int dataHash)
		{
			backupByFileCopy(application(), dataHash);
		}

		public void RestoreApplicationDatabase(int dataHash)
		{
			restoreByFileCopy(application(), dataHash);
		}

		public void BackupApplicationDatabaseBySql(string path, int dataHash)
		{
			var database = application();
			database.BackupBySql().Backup(path, database.BackupNameForBackup(dataHash));
		}

		public bool TryRestoreApplicationDatabaseBySql(string path, int dataHash)
		{
			var database = application();
			return database.BackupBySql().TryRestore(path, database.BackupNameForRestore(dataHash));
		}

		public void BackupAnalyticsDatabase(int dataHash)
		{
			backupByFileCopy(analytics(), dataHash);
		}

		public void RestoreAnalyticsDatabase(int dataHash)
		{
			restoreByFileCopy(analytics(), dataHash);
		}

		public void ClearAnalyticsData()
		{
			analytics().ConfigureSystem().CleanByAnalyticsProcedure();
		}

		public void BackupAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			database.BackupBySql().Backup(path, database.BackupNameForBackup(dataHash));
		}

		public bool TryRestoreAnalyticsDatabaseBySql(string path, int dataHash)
		{
			var database = analytics();
			return database.BackupBySql().TryRestore(path, database.BackupNameForRestore(dataHash));
		}

		private void createOrRestoreApplication(string tenant) =>
			createOrRestore(application(), tenant);

		private void createOrRestoreAnalytics() =>
			createOrRestore(analytics(), null);

		private void createOrRestoreAgg() =>
			createOrRestore(agg(), null);

		private void createOrRestore(DatabaseHelper database, string tenant)
		{
			if (tryRestoreByFileCopy(database, 0))
				return;
			createDatabase(database, tenant);
			backupByFileCopy(database, 0);
		}

		private static void createDatabase(DatabaseHelper database, string tenant)
		{
			database.CreateByDbManager();
			database.CreateSchemaByDbManager();

			if (database.DatabaseType != DatabaseType.TeleoptiCCC7) return;

			var configure = database.ConfigureSystem();

			//would be better if dbmanager was called, but don't have the time right now....
			// eh, that thing that is called IS the db manager!
			// I think this is because "normally" MergePersonAssignments proc is called from security tool on upgrade...
			// .. and that proc makes database schema changes.
			configure.MergePersonAssignments();

			configure.PersistAuditSetting();
			
			setTenantInfo(database);
		}

		private void backupByFileCopy(DatabaseHelper database, int dataHash)
		{
			var name = database.BackupNameForBackup(dataHash);
			var backup = database.BackupByFileCopy().Backup(name);
			var file = Path.Combine(_repositoryRoot, name);
			File.WriteAllText(file, JsonConvert.SerializeObject(backup, Formatting.Indented));
		}

		private bool tryRestoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
			if (!database.Tasks().Exists(database.DatabaseName))
				return false;

			var name = database.BackupNameForRestore(dataHash);
			var file = Path.Combine(_repositoryRoot, name);
			if (!File.Exists(file))
				return false;

			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(file));
			var result = database.BackupByFileCopy().TryRestore(backup);

			setTenantInfo(database);

			return result;
		}

		private void restoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			var name = database.BackupNameForRestore(dataHash);
			var file = Path.Combine(_repositoryRoot, name);
			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(file));
			var result = database.BackupByFileCopy().TryRestore(backup);
			if (!result)
				throw new Exception("Restore failed!");

			setTenantInfo(database);
		}

		private static void setTenantInfo(DatabaseHelper database)
		{
			if (database.DatabaseType != DatabaseType.TeleoptiCCC7) 
				return;
			var configure = database.ConfigureSystem();
			configure.SetTenantConnectionInfo(InfraTestConfigReader.TenantName(), database.ConnectionString, analytics().ConnectionString);
		}

		private static DatabaseHelper application()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.ApplicationConnectionString(),
				DatabaseType.TeleoptiCCC7,
				new DbManagerLog4Net("DbManager.Application"),
				new WfmInstallationEnvironment()
			) {DbManagerFolderPath = DbManagerFolderLocator.LocateDatabaseFolderUsingBlackMagic(TestContext.CurrentContext.TestDirectory)};
		}

		private static DatabaseHelper agg()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AggConnectionString(),
				DatabaseType.TeleoptiCCCAgg,
				new DbManagerLog4Net("DbManager.Agg"),
				new WfmInstallationEnvironment()
			) {DbManagerFolderPath = DbManagerFolderLocator.LocateDatabaseFolderUsingBlackMagic(TestContext.CurrentContext.TestDirectory)};
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AnalyticsConnectionString(),
				DatabaseType.TeleoptiAnalytics,
				new DbManagerLog4Net("DbManager.Analytics"),
				new WfmInstallationEnvironment()
			) {DbManagerFolderPath = DbManagerFolderLocator.LocateDatabaseFolderUsingBlackMagic(TestContext.CurrentContext.TestDirectory)};
		}

		public void CreateFirstTenantAdminUser()
		{
			var database = application();
			database.ConfigureSystem().TryAddTenantAdminUser();
		}
	}
}