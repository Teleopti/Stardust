using System;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain;
using Teleopti.Support.Library;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Ccc.TestCommon
{
	public class DatabaseTestHelper
	{
		public void CreateDatabases(string tenant = TestTenantName.Name)
		{
			using (testDirectoryFix())
			{
				createOrRestoreApplication(tenant);
				createOrRestoreAnalytics();
				createOrRestoreAgg();
			}
		}

		public void BackupApplicationDatabase(int dataHash)
		{
			using (testDirectoryFix())
				backupByFileCopy(application(), dataHash);
		}

		public void RestoreApplicationDatabase(int dataHash)
		{
			using (testDirectoryFix())
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
			using (testDirectoryFix())
				backupByFileCopy(analytics(), dataHash);
		}

		public void RestoreAnalyticsDatabase(int dataHash)
		{
			using (testDirectoryFix())
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

		private static void createOrRestoreApplication(string tenant) =>
			createOrRestore(application(), tenant);

		private static void createOrRestoreAnalytics() =>
			createOrRestore(analytics(), null);

		private static void createOrRestoreAgg() =>
			createOrRestore(agg(), null);

		private static void createOrRestore(DatabaseHelper database, string tenant)
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
			configure.SetTenantConnectionInfo(tenant, database.ConnectionString, analytics().ConnectionString);
		}

		private static void backupByFileCopy(DatabaseHelper database, int dataHash)
		{
			var name = database.BackupNameForBackup(dataHash);
			var backup = database.BackupByFileCopy().Backup(name);
			File.WriteAllText(name, JsonConvert.SerializeObject(backup, Formatting.Indented));
		}

		private static bool tryRestoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			// maybe it would be possible to attach it if a file exists but the database doesnt. but wth..
			if (!database.Tasks().Exists(database.DatabaseName))
				return false;

			var name = database.BackupNameForRestore(dataHash);
			if (!File.Exists(name))
				return false;

			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(name));
			return database.BackupByFileCopy().TryRestore(backup);
		}

		private static void restoreByFileCopy(DatabaseHelper database, int dataHash)
		{
			var backup = JsonConvert.DeserializeObject<Backup>(File.ReadAllText(database.BackupNameForRestore(dataHash)));
			var result = database.BackupByFileCopy().TryRestore(backup);
			if (!result)
				throw new Exception("Restore failed!");
		}

		private static DatabaseHelper application()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.ApplicationConnectionString(),
				DatabaseType.TeleoptiCCC7,
				new DbManagerLog4Net("DbManager.Application"),
				new WfmInstallationEnvironment()
			);
		}

		private static DatabaseHelper agg()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AggConnectionString(),
				DatabaseType.TeleoptiCCCAgg,
				new DbManagerLog4Net("DbManager.Agg"),
				new WfmInstallationEnvironment()
			);
		}

		private static DatabaseHelper analytics()
		{
			return new DatabaseHelper(
				InfraTestConfigReader.AnalyticsConnectionString(),
				DatabaseType.TeleoptiAnalytics,
				new DbManagerLog4Net("DbManager.Analytics"),
				new WfmInstallationEnvironment()
			);
		}

		private static IDisposable testDirectoryFix()
		{
			var path = Directory.GetCurrentDirectory();
			Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);
			return new GenericDisposable(() => { Directory.SetCurrentDirectory(path); });
		}

		public void CreateFirstTenantAdminUser()
		{
			var database = application();
			database.ConfigureSystem().TryAddTenantAdminUser();
		}
	}
}