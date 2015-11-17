using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
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
						createLogin(_commandLineArgument.appUserName, _commandLineArgument.appUserPwd, _commandLineArgument.isWindowsGroupName, databaseFolder, sqlVersion, masterExecuteSql, executeSql);
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
						createPermissions(_commandLineArgument.appUserName, sqlVersion, databaseFolder, executeSql);
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
								applyAzureStartDdl(_commandLineArgument.TargetDatabaseTypeName, databaseFolder, executeSql);
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

		private void applyAzureStartDdl(string databaseTypeName, DatabaseFolder folder, ExecuteSql executeSql)
		{
			logWrite("Applying Azure DDL starting point...");

			string fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\{1}.00000329.sql",
													  folder.Path(), databaseTypeName);
			string sql = File.ReadAllText(fileName);
			executeSql.ExecuteNonQuery(sql,Timeouts.CommandTimeout);
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

		private bool dbUserExist(string sqlLogin, ExecuteSql executeSql)
		{
			const string sql = @"select count(*) from sys.sysusers where name = @SQLLogin";
			return Convert.ToBoolean(executeSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlLogin } }));
		}

		private bool azureLoginExist(string sqlLogin, ExecuteSql masterExecuteSql)
		{
			const string sql = @"select count(*) from sys.sql_logins where name = @SQLLogin";
			return Convert.ToBoolean(masterExecuteSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlLogin } }));
		}

		private bool azureDatabaseUserExist(string sqlUser, ExecuteSql executeSql)
		{
			const string sql = @"select count(*) from sys.database_principals where name=@SQLLogin AND authentication_type=2";
			return Convert.ToBoolean(executeSql.ExecuteScalar(sql, parameters: new Dictionary<string, object> { { "@SQLLogin", sqlUser } }));
		}

		private void createDB(string databaseName, DatabaseType databaseType, SqlVersion sqlVersion, DatabaseCreator creator)
		{
			logWrite("Creating database " + databaseName + "...");

			if (sqlVersion.IsAzure)
				creator.CreateAzureDatabase(databaseType, databaseName);
			else
				creator.CreateDatabase(databaseType, databaseName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private bool sqlVersionGreaterThen(string checkVersion, ExecuteSql executeSql)
		{
			string serverVersion = string.Empty;
			executeSql.ExecuteCustom(sqlConnection => { serverVersion = sqlConnection.ServerVersion; });
			
			var serverVersionDetails = serverVersion.Split(new[] { "." }, StringSplitOptions.None);
			var checkVersionDetails = checkVersion.Split(new[] { "." }, StringSplitOptions.None);

			if (checkVersionDetails.Length < 3 && serverVersionDetails.Length < 3)
				throw new Exception("Unknown version string given from SQL Server or in code");

			var majorVersionNumber = int.Parse(serverVersionDetails[0], CultureInfo.InvariantCulture);
			var minorVersionNumber = int.Parse(serverVersionDetails[1], CultureInfo.InvariantCulture);
			var buildVersionNumber = int.Parse(serverVersionDetails[2], CultureInfo.InvariantCulture);

			var majorCheckVersionNumber = int.Parse(checkVersionDetails[0], CultureInfo.InvariantCulture);
			var minorCheckVersionNumber = int.Parse(checkVersionDetails[1], CultureInfo.InvariantCulture);
			var buildCheckVersionNumber = int.Parse(checkVersionDetails[2], CultureInfo.InvariantCulture);

			if (majorCheckVersionNumber < majorVersionNumber)
				return true;
			if (minorCheckVersionNumber < minorVersionNumber)
				return true;
			if (buildCheckVersionNumber < buildVersionNumber)
				return true;
			
			return false;
		}

		private void createLogin(string user, string pwd, Boolean iswingroup, DatabaseFolder folder, SqlVersion sqlVersion, ExecuteSql masterExecuteSql, ExecuteSql executeSql)
		{
			//TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
			string sql;
			if (sqlVersion.IsAzure)
			{
				if (iswingroup)
					masterExecuteSql.ExecuteNonQuery("PRINT 'Windows Logins cannot be added to Windows Azure for the momement'");
				else
				{
					if (sqlVersion.ProductVersion >= 12)
					{
						if (azureLoginExist(user, masterExecuteSql))
						{
							masterExecuteSql.ExecuteTransactionlessNonQuery(string.Format("DROP LOGIN [{0}]", user));
						}
						if (azureDatabaseUserExist(user, executeSql))
							sql = string.Format("ALTER USER [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						else
							sql = string.Format("CREATE USER [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						executeSql.ExecuteNonQuery(sql);
					}
					else
					{
						if (azureLoginExist(user, masterExecuteSql))
							sql = string.Format("ALTER LOGIN [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						else
							sql = string.Format("CREATE LOGIN [{0}] WITH PASSWORD=N'{1}'", user, pwd);
						masterExecuteSql.ExecuteTransactionlessNonQuery(sql);
					}
				}
			}
			else
			{
				string fileName;
				if (iswingroup)
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", folder.Path());
				else
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\SQL Logins - Create.sql", folder.Path());

				sql = File.ReadAllText(fileName);
				sql = sql.Replace("$(SQLLOGIN)", user);
				sql = sql.Replace("$(SQLPWD)", pwd);
				sql = sql.Replace("$(WINLOGIN)", user);
				masterExecuteSql.ExecuteTransactionlessNonQuery(sql);
			}

			logWrite("Created login!");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private  void createPermissions(string user, SqlVersion sqlVersion, DatabaseFolder folder, ExecuteSql executeSql)
		{
			
			//if appication login = sa then don't bother to do anything
			if (compareStringLowerCase(user, @"sa"))
				return;

			if ((sqlVersion.IsAzure && sqlVersion.ProductVersion < 12) || !sqlVersion.IsAzure)
			{
				//Create or Re-link e.g Alter the DB-user from SQL-Login
				var createDBUser = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] FOR LOGIN [{0}]", user);

				string relinkSqlUser;
				if (sqlVersionGreaterThen(SQL2005SP2, executeSql))
					relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"ALTER USER [{0}] WITH LOGIN = [{0}]", user);
				else
					relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"sp_change_users_login 'Update_One', '{0}', '{0}'", user);

				if (dbUserExist(user, executeSql))
				{
					logWrite("DB user already exist, re-link ...");
					executeSql.ExecuteTransactionlessNonQuery(relinkSqlUser);
				}
				else
				{
					logWrite("DB user is missing. Create DB user ...");
					executeSql.ExecuteTransactionlessNonQuery(createDBUser);
				}
			}

			//Add permission
			var fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", folder.Path());

			var sql = File.ReadAllText(fileName);

			sql = sql.Replace("$(LOGIN)", user);

			executeSql.ExecuteNonQuery(sql);

			logWrite("Created Permissions!");
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
			return (string.Compare(stringA, stringB, true,
				 CultureInfo.CurrentCulture) == 0);
		}

		public void SetLogger(IUpgradeLog logger)
		{
			_logger = logger;
		}
	}

	public class FileLogger : IUpgradeLog
	{
		private readonly TextWriter _logFile;

		public FileLogger()
		{
			var nowDateTime = DateTime.Now;
			_logFile = new StreamWriter(string.Format(CultureInfo.CurrentCulture, "DBManagerLibrary_{0}_{1}.log", nowDateTime.ToString("yyyyMMdd", CultureInfo.CurrentCulture), nowDateTime.ToString("hhmmss", CultureInfo.CurrentCulture)));
		}

		public void Write(string message)
		{
			Console.Out.WriteLine(message);
			_logFile.WriteLine(message);
		}

		public void Write(string message, string level)
		{
			Write(message);
		}

		public void Dispose()
		{
			if (_logFile == null)
				return;
			_logFile.Close();
			_logFile.Dispose();
		}
	}
}