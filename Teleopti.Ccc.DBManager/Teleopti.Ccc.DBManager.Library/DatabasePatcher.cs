using System;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Support.Library;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabasePatcher
	{
		private IUpgradeLog _log;

		public DatabasePatcher(IUpgradeLog log)
		{
			_log = log ?? new NullLog();
		}

		public int Run(PatchCommand command)
		{
			try
			{
				var context = new PatchContext(command, _log);

				var safeMode = true;

				_log.Write("Running from:" + context.DatabaseFolder().Path());

				//If Permission Mode: check if application user name and password/windowsgroup is submitted
				if (command.CreatePermissions)
				{
					if (!(command.AppUserName.Length > 0 && (command.AppUserPassword.Length > 0 || command.IsWindowsGroupName)))
						throw new Exception("No Application user/Windows group name submitted!");
				}

				if (context.SqlVersion().IsAzure && command.IsWindowsGroupName)
					throw new Exception("Windows Azure don't support Windows Login for the moment!");

				//special for Azure => fn_my_permissions does not exist: http://msdn.microsoft.com/en-us/library/windowsazure/ee336248.aspx
				bool isSrvDbCreator;
				bool isSrvSecurityAdmin;
				if (context.SqlVersion().IsAzure)
				{
					isSrvDbCreator = true;
					isSrvSecurityAdmin = true;
				}
				else
				{
					isSrvDbCreator = context.HasSrvDbCreator();
					isSrvSecurityAdmin = context.HasSrvSecurityAdmin();
				}

				if (command.CreateDatabase && !((isSrvDbCreator && isSrvSecurityAdmin)))
					throw new Exception("Either sysAdmin or dbCreator + SecurityAdmin permissions are needed to install WFM databases!");

				if (!context.LoginExist() && !isSrvSecurityAdmin)
					throw new Exception(string.Format(CultureInfo.CurrentCulture, "The login '{0}' does not exist or wrong password supplied, and You don't have the permission to create/alter the login.", command.AppUserName));

				//Same sql login for admin and end user?
				if (command.CreatePermissions &&
					!command.UseIntegratedSecurity &&
					command.AppUserName.Length > 0 && command.UserName.Length > 0 &&
					command.AppUserName.ToLower() == command.UserName.ToLower()
				)
				{
					safeMode = false;
					_log.Write("Warning: The application will have db_owner permissions. Consider using a different login for the end users!");
					Thread.Sleep(1200);
				}

				//Exclude Agg from Azure
				if (context.SqlVersion().IsAzure && command.DatabaseType == DatabaseType.TeleoptiCCCAgg)
				{
					_log.Write("This is a TeleoptiCCCAgg, exclude from SQL Azure");
					return 0;
				}

				if (command.DropDatabase)
					dropDatabase(command, context);

				var willTryToCreateDatabase =
						command.CreateDatabase ||
						command.RecreateDatabaseIfNotExistsOrNewer ||
						!string.IsNullOrEmpty(command.RestoreBackup) ||
						!string.IsNullOrEmpty(command.RestoreBackupIfNotExistsOrNewer)
					;
				if (!willTryToCreateDatabase && command.DropDatabase)
				{
					// if it will drop and not create, there's nothing to update, and its finished.
					_log.Write("Finished !");
					return 0;
				}

				if (command.CreateDatabase)
					createDatabase(command, context);

				if (command.RecreateDatabaseIfNotExistsOrNewer)
					ifNotExistOrNewer(command, context, () =>
					{
						_log.Write("Recreating database " + command.DatabaseName + "...");
						dropDatabase(command, context);
						createDatabase(command, context);
					});

				if (!string.IsNullOrEmpty(command.RestoreBackup))
				{
					_log.Write("Restoring database " + command.DatabaseName + "...");
					context.Restorer().Restore(command);
				}

				if (!string.IsNullOrEmpty(command.RestoreBackupIfNotExistsOrNewer))
					ifNotExistOrNewer(command, context, () =>
					{
						_log.Write("Restoring database " + command.DatabaseName + "...");
						context.Restorer().Restore(command);
					});

				if (!context.DatabaseExists())
				{
					_log.Write("Database " + command.DatabaseName + " does not exist on server " + command.ServerName);
					_log.Write("Run DBManager.exe with -c switch to create.");
					return 0;
				}

				//Try create or re-create login
				if (command.CreatePermissions && safeMode && isSrvSecurityAdmin)
					new LoginHelper(_log, context.MasterExecuteSql(), context.DatabaseFolder())
						.CreateLogin(command.AppUserName, command.AppUserPassword, command.IsWindowsGroupName, context.SqlVersion());

				//if we are not db_owner, bail out
				if (!context.IsDbOwner())
					throw new Exception("db_owner or sysAdmin permissions needed to patch the database!");

				//Set permissions of the newly application user on db.
				if (command.CreatePermissions && safeMode)
					new PermissionsHelper(_log, context.DatabaseFolder(), context.ExecuteSql())
						.CreatePermissions(command.AppUserName, command.AppUserPassword, context.SqlVersion());

				if (command.UpgradeDatabase)
				{
					if (context.VersionTableExists())
					{
						//Shortcut to release 329, Azure specific script
						if (context.SqlVersion().IsAzure && context.DatabaseVersionInformation().GetDatabaseVersion() == 0)
							new AzureStartDDL(context.DatabaseFolder(), context.ExecuteSql())
								.Apply((DatabaseType) Enum.Parse(typeof(DatabaseType), command.DatabaseTypeName));

						new DatabaseSchemaCreator(
								context.DatabaseVersionInformation(),
								context.SchemaVersionInformation(),
								context.ExecuteSql(),
								context.DatabaseFolder(),
								_log)
							.Create(command.DatabaseType, context.SqlVersion());
					}
					else
					{
						_log.Write("Version information is missing, is this a Raptor db?");
					}
				}

				_log.Write("Finished !");

				return 0;
			}
			catch (Exception e)
			{
				_log.Write($"An error occurred: {e}\n{e.StackTrace}", "ERROR");
				return -1;
			}
			finally
			{
				_log.Dispose();
			}
		}

		private static void ifNotExistOrNewer(PatchCommand command, PatchContext context, Action action)
		{
			if (!context.DatabaseExists())
				action.Invoke();
			else
			{
				var existingDatabaseIsNewer = context.DatabaseVersionInformation().GetDatabaseVersion() > context.SchemaVersionInformation().GetSchemaVersion(command.DatabaseType);
				if (existingDatabaseIsNewer)
					action.Invoke();
			}
		}

		private void dropDatabase(PatchCommand command, PatchContext context)
		{
			_log.Write("Dropping database " + command.DatabaseName + "...");
			new DatabaseDropper(context.MasterExecuteSql())
				.DropDatabaseIfExists(command.DatabaseName);
		}

		private void createDatabase(PatchCommand command, PatchContext context)
		{
			_log.Write("Creating database " + command.DatabaseName + "...");
			var creator = new DatabaseCreator(context.DatabaseFolder(), context.ExecuteSql(), context.MasterExecuteSql());
			if (context.SqlVersion().IsAzure)
				creator.CreateAzureDatabase(command.DatabaseType, command.DatabaseName);
			else
				creator.CreateDatabase(command.DatabaseType, command.DatabaseName);
			_log.Write("Creating database version table and setting initial version to 0...");
			context.DatabaseVersionInformation().CreateTable();
		}

		public void SetLogger(IUpgradeLog logger)
		{
			_log = logger;
		}
	}
}