﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabasePatcher
	{
		private IUpgradeLog _logger;

		public DatabasePatcher(IUpgradeLog upgradeLog)
		{
			_logger = upgradeLog;
		}
		
		public int Run(CommandLineArgument commandLineArgument)
		{
			try
			{
				var safeMode = true;
				var isAzure = commandLineArgument.ServerName.Contains(".database.windows.net");

				var databaseFolder = new DatabaseFolder(new DbManagerFolder(commandLineArgument.PathToDbManager));
				var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
				_logger.Write("Running from:" + databaseFolder.Path());

				//If Permission Mode: check if application user name and password/windowsgroup is submitted
				if (commandLineArgument.PermissionMode)
				{
					if (!(commandLineArgument.appUserName.Length > 0 && (commandLineArgument.appUserPwd.Length > 0 || commandLineArgument.isWindowsGroupName)))
						throw new Exception("No Application user/Windows group name submitted!");
				}

				if (isAzure && commandLineArgument.isWindowsGroupName)
					throw new Exception("Windows Azure don't support Windows Login for the moment!");

				//special for Azure => fn_my_permissions does not exist: http://msdn.microsoft.com/en-us/library/windowsazure/ee336248.aspx
				bool isSrvDbCreator;
				bool isSrvSecurityAdmin;
				ExecuteSql masterExecuteSql = null;
				if (isAzure)
				{
					isSrvDbCreator = true;
					isSrvSecurityAdmin = true;
				}
				else
				{
					masterExecuteSql = new ExecuteSql(() => connectAndOpen(commandLineArgument.ConnectionStringToMaster), _logger);
					isSrvDbCreator = hasSrvDbCreator(masterExecuteSql);
					isSrvSecurityAdmin = hasSrvSecurityAdmin(masterExecuteSql);
				}

				//try connect to DB using given AppLogin
				var loginExist = false;
				if (!commandLineArgument.isWindowsGroupName) //SQL
				{
					try
					{
						using (connectAndOpen(commandLineArgument.ConnectionStringAppLogOn(isAzure ? commandLineArgument.DatabaseName : DatabaseHelper.MasterDatabaseName)))
						{
						}

						loginExist = true;
					}
					catch
					{
						loginExist = false;
					}
				}
				else if (!isAzure) //Win 
					loginExist = verifyWinGroup(commandLineArgument.appUserName, masterExecuteSql); //We will need to check on sys.syslogins
				
				//New installation
				if (commandLineArgument.WillCreateNewDatabase && !((isSrvDbCreator && isSrvSecurityAdmin)))
				{
					throw new Exception("Either sysAdmin or dbCreator + SecurityAdmin permissions are needed to install WFM databases!");
				}

				//patch
				if (!loginExist && !isSrvSecurityAdmin)
				{
					throw new Exception(string.Format(CultureInfo.CurrentCulture, "The login '{0}' does not exist or wrong password supplied, and You don't have the permission to create/alter the login.", commandLineArgument.appUserName));
				}

				//Same sql login for admin and end user?
				if ((commandLineArgument.PermissionMode) &&
					 (!commandLineArgument.UseIntegratedSecurity) &&
					 (commandLineArgument.appUserName.Length > 0 && commandLineArgument.UserName.Length > 0) &&
			 (compareStringLowerCase(commandLineArgument.appUserName, commandLineArgument.UserName))
					 )
				{
					safeMode = false;
					_logger.Write("Warning: The application will have db_owner permissions. Consider using a different login for the end users!");
					System.Threading.Thread.Sleep(1200);
				}

				//Exclude Agg from Azure
				if (isAzure && commandLineArgument.TargetDatabaseTypeName == DatabaseType.TeleoptiCCCAgg.ToString())
				{
					_logger.Write("This is a TeleoptiCCCAgg, exclude from SQL Azure");
					return 0;
				}

				if (commandLineArgument.WillCreateNewDatabase)
				{
					if(masterExecuteSql == null)
						masterExecuteSql = new ExecuteSql(() => connectAndOpen(commandLineArgument.ConnectionStringToMaster), _logger);
					createDB(commandLineArgument.DatabaseName, commandLineArgument.TargetDatabaseType, isAzure, new DatabaseCreator(databaseFolder, masterExecuteSql));
				}
				ExecuteSql executeSql;
				try
				{
					executeSql = new ExecuteSql(() => connectAndOpen(commandLineArgument.ConnectionString), _logger);
				}
				catch
				{
					_logger.Write("Database " + commandLineArgument.DatabaseName + " does not exist on server " + commandLineArgument.ServerName);
					_logger.Write("Run DBManager.exe with -c switch to create.");
					return 0;
				}


				//Try create or re-create login
				
					if (commandLineArgument.PermissionMode && safeMode && isSrvSecurityAdmin)
					{
						var loginHelper = new LoginHelper(_logger, masterExecuteSql, databaseFolder);
						loginHelper.CreateLogin(commandLineArgument.appUserName, commandLineArgument.appUserPwd, commandLineArgument.isWindowsGroupName, isAzure);
					}

					var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder, executeSql);

					//if we are not db_owner, bail out
					if (!isDbOwner(executeSql))
					{
						throw new Exception("db_owner or sysAdmin permissions needed to patch the database!");
					}

					//if this is the very first create, add VersionControl table
					if (commandLineArgument.WillCreateNewDatabase)
					{
						createDefaultVersionInformation(databaseVersionInformation);
					}

					//Set permissions of the newly application user on db.
					if (commandLineArgument.PermissionMode && safeMode)
					{
						var permissionsHelper = new PermissionsHelper(_logger, databaseFolder, executeSql);
						permissionsHelper.CreatePermissions(commandLineArgument.appUserName, commandLineArgument.appUserPwd, isAzure);
					}

					//Patch database
					if (commandLineArgument.PatchMode)
					{
						//Does the Version Table exist?
						if (versionTableExists(executeSql))
						{
							//Shortcut to release 329, Azure specific script
							if (isAzure && databaseVersionInformation.GetDatabaseVersion() == 0)
							{
								new AzureStartDDL(databaseFolder, executeSql).Apply((DatabaseType)Enum.Parse(typeof(DatabaseType), commandLineArgument.TargetDatabaseTypeName));
							}

							var dbCreator = new DatabaseSchemaCreator(databaseVersionInformation,
								schemaVersionInformation, executeSql, databaseFolder, _logger);
							dbCreator.Create(commandLineArgument.TargetDatabaseType, isAzure);
						}
						else
						{
							_logger.Write("Version information is missing, is this a Raptor db?");
						}
					}

				_logger.Write("Finished !");

				return 0;
			}
			catch (Exception e)
			{
				_logger.Write(string.Format("An error occurred: {0}\n{1}", e, e.StackTrace), "ERROR");
				return -1;
			}
			finally
			{
				_logger.Dispose();
			}
		}

		private void sqlConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			_logger.Write(e.Message);
		}

		private bool versionTableExists(ExecuteSql executeSql)
		{
			const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'DatabaseVersion'";
			return Convert.ToBoolean(executeSql.ExecuteScalar(sql));
		}

		private bool isDbOwner(ExecuteSql executeSql)
		{
			// Testing workaround to Internal .Net Framework Data Provider error 6. error
			bool result = false;
			for(var attempt=0;attempt<5; attempt++)
				try
				{
					result = _isDbOwner(executeSql);
					break;
				}
				catch (Exception)
				{
					Thread.Sleep(TimeSpan.FromSeconds(5));
					continue;
				}
			return result;
		}

		private bool _isDbOwner(ExecuteSql executeSql)
		{
			const string sql = "select IS_MEMBER ('db_owner')";
			return Convert.ToBoolean(executeSql.ExecuteScalar(sql));
		}

		private bool hasSrvSecurityAdmin(ExecuteSql masterExecuteSql)
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='ALTER ANY LOGIN'";
			return Convert.ToBoolean(masterExecuteSql.ExecuteScalar(sql));
		}

		private bool hasSrvDbCreator(ExecuteSql masterExecuteSql)
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='CREATE ANY DATABASE'";
			return Convert.ToBoolean(masterExecuteSql.ExecuteScalar(sql));
		}

		private bool verifyWinGroup(string winNtGroup, ExecuteSql masterExecuteSql)
		{
			const string sql = @"SELECT count(name) from sys.syslogins where isntgroup = 1 and name = @WinNTGroup";
			return Convert.ToBoolean(masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@WinNTGroup", winNtGroup } }));
		}

		private void createDB(string databaseName, DatabaseType databaseType, bool isAzure, DatabaseCreator creator)
		{
			_logger.Write("Creating database " + databaseName + "...");

			if (isAzure)
				creator.CreateAzureDatabase(databaseType, databaseName);
			else
				creator.CreateDatabase(databaseType, databaseName);
		}

		private void createDefaultVersionInformation(DatabaseVersionInformation databaseVersionInformation)
		{
			_logger.Write("Creating database version table and setting inital version to 0...");
			databaseVersionInformation.CreateTable();
		}

		private SqlConnection connectAndOpen(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			sqlConnection.InfoMessage += sqlConnectionInfoMessage;
			return sqlConnection;
		}

		private static bool compareStringLowerCase(string stringA, string stringB)
		{
			return string.Compare(stringA, stringB, true, CultureInfo.CurrentCulture) == 0;
		}

		public void SetLogger(IUpgradeLog logger)
		{
			_logger = logger;
		}
	}
}