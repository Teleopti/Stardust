using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabasePatcher
	{
		private CommandLineArgument _commandLineArgument;

		private const string SQL2005SP2 = "9.00.3042"; //http://www.sqlteam.com/article/sql-server-versions
		private IUpgradeLog _logger;
		
		public DatabasePatcher(IUpgradeLog upgradeLog)
		{
			_logger = upgradeLog;
		}

		public int Run(CommandLineArgument arguments)
		{
			_commandLineArgument = arguments;

			try
			{
				bool safeMode = true;

				var databaseFolder = new DatabaseFolder(new DbManagerFolder(_commandLineArgument.PathToDbManager));
				var schemaVersionInformation = new SchemaVersionInformation(databaseFolder);
				logWrite("Running from:" + databaseFolder.Path());

				//If Permission Mode: check if application user name and password/windowsgroup is submitted
				if (_commandLineArgument.PermissionMode)
				{
					if (!(_commandLineArgument.appUserName.Length > 0 && (_commandLineArgument.appUserPwd.Length > 0 || _commandLineArgument.isWindowsGroupName)))
						throw new Exception("No Application user/Windows group name submitted!");
				}

				//Check Azure
				var masterExecuteSql = new ExecuteSql(() => connectAndOpen(_commandLineArgument.ConnectionStringToMaster), _logger);
				var databaseHelper = new ServerVersionHelper(masterExecuteSql);
				var sqlVersion = databaseHelper.Version();

				if (sqlVersion.IsAzure && _commandLineArgument.isWindowsGroupName)
					throw new Exception("Windows Azure don't support Windows Login for the moment!");

				//special for Azure => fn_my_permissions does not exist: http://msdn.microsoft.com/en-us/library/windowsazure/ee336248.aspx
				bool isSrvDbCreator;
				bool isSrvSecurityAdmin;
				if (sqlVersion.IsAzure)
				{
					isSrvDbCreator = true;
					isSrvSecurityAdmin = true;
				}
				else
				{
					isSrvDbCreator = hasSrvDbCreator(masterExecuteSql);
					isSrvSecurityAdmin = hasSrvSecurityAdmin(masterExecuteSql);
				}

				//try connect to DB using given AppLogin
				bool loginExist;
				if (!_commandLineArgument.isWindowsGroupName) //SQL
				{
					try
					{
						using(connectAndOpen(_commandLineArgument.ConnectionStringAppLogOn(sqlVersion.IsAzure ? _commandLineArgument.DatabaseName : "master")))
						{
						}
						
						loginExist = true;
					}
					catch
					{
						loginExist = false;
					}
				}
				else //Win
					loginExist = verifyWinGroup(_commandLineArgument.appUserName, masterExecuteSql); //We will need to check on sys.syslogins

				//New installation
				if (_commandLineArgument.WillCreateNewDatabase && !((isSrvDbCreator && isSrvSecurityAdmin)))
				{
					throw new Exception("Either sysAdmin or dbCreator + SecurityAdmin permissions are needed to install WFM databases!");
				}

				//patch
				if (!loginExist && !isSrvSecurityAdmin)
				{
					throw new Exception(string.Format(CultureInfo.CurrentCulture, "The login '{0}' does not exist or wrong password supplied, and You don't have the permission to create/alter the login.", _commandLineArgument.appUserName));
				}

				//Same sql login for admin and end user?
				if ((_commandLineArgument.PermissionMode) &&
					 (!_commandLineArgument.UseIntegratedSecurity) &&
					 (_commandLineArgument.appUserName.Length > 0 && _commandLineArgument.UserName.Length > 0) &&
			 (compareStringLowerCase(_commandLineArgument.appUserName, _commandLineArgument.UserName))
					 )
				{
					safeMode = false;
					logWrite("Warning: The application will have db_owner permissions. Consider using a different login for the end users!");
					System.Threading.Thread.Sleep(1200);
				}

				//Exclude Agg from Azure
				if (sqlVersion.IsAzure && _commandLineArgument.TargetDatabaseTypeName == DatabaseType.TeleoptiCCCAgg.ToString())
				{
					logWrite("This is a TeleoptiCCCAgg, exclude from SQL Azure");
					return 0;
				}

				if (_commandLineArgument.WillCreateNewDatabase)
				{
					createDB(_commandLineArgument.DatabaseName, _commandLineArgument.TargetDatabaseType, sqlVersion, new DatabaseCreator(databaseFolder, masterExecuteSql));
				}

				//Does the db exist?
				if (databaseExists(masterExecuteSql))
				{
					//Try create or re-create login
					var executeSql = new ExecuteSql(() => connectAndOpen(_commandLineArgument.ConnectionString), _logger);
					if (_commandLineArgument.PermissionMode && safeMode && isSrvSecurityAdmin)
					{
						var loginHelper = new LoginHelper(_logger, masterExecuteSql, executeSql, databaseFolder);
						loginHelper.CreateLogin(_commandLineArgument.appUserName, _commandLineArgument.appUserPwd, _commandLineArgument.isWindowsGroupName, sqlVersion);
					}

					var databaseVersionInformation = new DatabaseVersionInformation(databaseFolder,executeSql);

					//if we are not db_owner, bail out
					if (!isDbOwner(executeSql))
					{
						throw new Exception("db_owner or sysAdmin permissions needed to patch the database!");
					}

					//if this is the very first create, add VersionControl table
					if (_commandLineArgument.WillCreateNewDatabase)
					{
						createDefaultVersionInformation(databaseVersionInformation);
					}

					//Set permissions of the newly application user on db.
					if (_commandLineArgument.PermissionMode && safeMode)
					{
						var permissionsHelper = new PermissionsHelper(_logger, databaseFolder, executeSql);
						permissionsHelper.CreatePermissions(_commandLineArgument.appUserName, sqlVersion);
					}

					//Patch database
					if (_commandLineArgument.PatchMode)
					{
						//Does the Version Table exist?
						if (versionTableExists(executeSql))
						{
							//Shortcut to release 329, Azure specific script
							if (sqlVersion.IsAzure && databaseVersionInformation.GetDatabaseVersion() == 0)
							{
								new AzureStartDDL(databaseFolder, executeSql).Apply((DatabaseType) Enum.Parse(typeof(DatabaseType),  _commandLineArgument.TargetDatabaseTypeName));
							}

							var dbCreator = new DatabaseSchemaCreator(databaseVersionInformation,
								schemaVersionInformation, executeSql, databaseFolder, _logger);
							dbCreator.Create(_commandLineArgument.TargetDatabaseType);
						}
						else
						{
							logWrite("Version information is missing, is this a Raptor db?");
						}
					}
				}
				else
				{
					logWrite("Database " + _commandLineArgument.DatabaseName + " does not exist on server " + _commandLineArgument.ServerName);
					logWrite("Run DBManager.exe with -c switch to create.");
				}

				logWrite("Finished !");

				return 0;
			}
         catch (Exception e)
			{
				logWrite(string.Format("An error occurred: {0}", e), "ERROR");
				return -1;
			}
			finally
			{
				_logger.Dispose();
			}
		}

		private void sqlConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			logWrite(e.Message);
		}

		private void logWrite(string s, string level = "DEBUG")
		{
			_logger.Write(s, level);
		}

		private bool versionTableExists(ExecuteSql executeSql)
		{
			const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'DatabaseVersion'";
			return Convert.ToBoolean(executeSql.ExecuteScalar(sql));
		}

		private bool databaseExists(ExecuteSql masterExecuteSql)
		{
			const string sql = "SELECT COUNT(*) AS DBExists FROM sys.databases WHERE [name]=@database_name";
			return Convert.ToBoolean(masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@database_name", _commandLineArgument.DatabaseName } }));
		}

		private bool isDbOwner(ExecuteSql executeSql)
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

		private void createDB(string databaseName, DatabaseType databaseType, SqlVersion sqlVersion, DatabaseCreator creator)
		{
			logWrite("Creating database " + databaseName + "...");

			if (sqlVersion.IsAzure)
				creator.CreateAzureDatabase(databaseType, databaseName);
			else
				creator.CreateDatabase(databaseType, databaseName);
		}

		private void createDefaultVersionInformation(DatabaseVersionInformation databaseVersionInformation)
		{
			logWrite("Creating database version table and setting inital version to 0...");
			databaseVersionInformation.CreateTable();
		}

		private SqlConnection connectAndOpen(string connectionString)
		{
			var sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			sqlConnection.InfoMessage += sqlConnectionInfoMessage;
			return sqlConnection;
		}

		 private bool compareStringLowerCase(string stringA, string stringB)
		{
			return string.Compare(stringA, stringB, true, CultureInfo.CurrentCulture) == 0;
		}

		public void SetLogger(IUpgradeLog logger)
		{
			_logger = logger;
		}
	}
}