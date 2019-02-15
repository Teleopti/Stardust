using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;
using Teleopti.Support.Security.Library;
using Teleopti.Wfm.Azure.Common;

namespace Teleopti.Develop.Batflow
{
	public enum DatabaseOperation
	{
		Skip,
		Ensure,
		Init,
		Drop
	}

	public class DatabaseFixer
	{
		private readonly IUpgradeLog _log;
		private readonly IInstallationEnvironment _installationEnvironment;

		public DatabaseFixer(IUpgradeLog log, IInstallationEnvironment installationEnvironment)
		{
			_log = log;
			_installationEnvironment = installationEnvironment;
		}

		public void Fix(FixDatabasesCommand command)
		{
			if (command.Operation() == DatabaseOperation.Skip)
				return;

			command.ConsoleStart();

			if (command.Operation() == DatabaseOperation.Ensure)
			{
				ensure(command, command.ApplicationDatabase(), command.ApplicationDatabaseBackup(), DatabaseType.TeleoptiCCC7);
				ensure(command, command.AnalyticsDatabase(), command.AnalyticsDatabaseBackup(), DatabaseType.TeleoptiAnalytics);
				ensure(command, command.AggDatabase(), command.AggDatabaseBackup(), DatabaseType.TeleoptiCCCAgg);
			}

			if (command.Operation() == DatabaseOperation.Init)
			{
				init(command, command.ApplicationDatabase(), command.ApplicationDatabaseBackup(), DatabaseType.TeleoptiCCC7);
				init(command, command.AnalyticsDatabase(), command.AnalyticsDatabaseBackup(), DatabaseType.TeleoptiAnalytics);
				init(command, command.AggDatabase(), command.AggDatabaseBackup(), DatabaseType.TeleoptiCCCAgg);
			}

			if (command.Operation() == DatabaseOperation.Drop)
			{
				drop(command, command.ApplicationDatabase(), DatabaseType.TeleoptiCCC7);
				drop(command, command.AnalyticsDatabase(), DatabaseType.TeleoptiAnalytics);
				drop(command, command.AggDatabase(), DatabaseType.TeleoptiCCCAgg);
			}

			if (command.Operation() != DatabaseOperation.Drop)
			{
				new UpgradeRunner(_log).Upgrade(new UpgradeCommand
				{
					Server = command.Server(),
					ApplicationDatabase = command.ApplicationDatabase(),
					AnalyticsDatabase = command.AnalyticsDatabase(),
					AggDatabase = command.AggDatabase(),
					UseIntegratedSecurity = true
				});

				new HangfireRecurringJobTrigger().Trigger(
					new HangfireRecurringJobTriggerCommand
					{
						Server = command.Server(),
						ApplicationDatabase = command.ApplicationDatabase(),
						AnalyticsDatabase = command.AnalyticsDatabase()
					}
				);
			}

			command.ConsoleEnd();
		}

		private void init(FixDatabasesCommand command, string database, string backup, DatabaseType type)
		{
			var patchCommand = new PatchCommand
			{
				DbManagerFolderPath = command.DatabaseSourcePath(),
				ServerName = command.Server(),
				DatabaseName = database,
				UseIntegratedSecurity = true,
				DatabaseType = type,
				UpgradeDatabase = true,
			};

			if (command.Operation() == DatabaseOperation.Init)
			{
				if (backup != null)
					patchCommand.RestoreBackup = backup;
				else
				{
					patchCommand.DropDatabase = true;
					patchCommand.CreateDatabase = true;
				}
			}

			new DatabasePatcher(_log, _installationEnvironment).Run(patchCommand);
		}

		private void ensure(FixDatabasesCommand command, string database, string backup, DatabaseType type)
		{
			var patchCommand = new PatchCommand
			{
				DbManagerFolderPath = command.DatabaseSourcePath(),
				ServerName = command.Server(),
				DatabaseName = database,
				UseIntegratedSecurity = true,
				DatabaseType = type,
				UpgradeDatabase = true,
			};

			if (command.Operation() == DatabaseOperation.Ensure)
			{
				if (backup != null)
					patchCommand.RestoreBackupIfNotExistsOrNewer = backup;
				else
					patchCommand.RecreateDatabaseIfNotExistsOrNewer = true;
			}

			new DatabasePatcher(_log, _installationEnvironment).Run(patchCommand);
		}

		private void drop(FixDatabasesCommand command, string database, DatabaseType type) =>
			new DatabasePatcher(_log, _installationEnvironment).Run(new PatchCommand
			{
				DbManagerFolderPath = command.DatabaseSourcePath(),
				ServerName = command.Server(),
				DatabaseName = database,
				UseIntegratedSecurity = true,
				DatabaseType = type,
				DropDatabase = true,
			});
	}
}