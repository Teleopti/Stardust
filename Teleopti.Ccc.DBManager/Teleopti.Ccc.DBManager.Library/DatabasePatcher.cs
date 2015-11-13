using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.DBManager.Library
{
	public class DatabasePatcher
	{
		private SqlConnection _sqlConnection;
		private SqlConnection _sqlConnectionAppLogin;
		private DatabaseFolder _databaseFolder;
		private CommandLineArgument _commandLineArgument;

		private bool _isAzure;
		private bool _isSrvDbCreator;
		private bool _isSrvSecurityAdmin;
		private bool _loginExist;

		private const string AzureEdition = "SQL Azure";
		private const string SQL2005SP2 = "9.00.3042"; //http://www.sqlteam.com/article/sql-server-versions
		public IUpgradeLog Logger;
		private DatabaseVersionInformation _databaseVersionInformation;
		private SchemaVersionInformation _schemaVersionInformation;


		public int Run(CommandLineArgument arguments)
		{
			_commandLineArgument = arguments;

			try
			{
				if( Logger == null)
					Logger = new MyLogger();
				
				bool SafeMode = true;

				_databaseFolder = new DatabaseFolder(new DbManagerFolder(_commandLineArgument.PathToDbManager));
				_schemaVersionInformation = new SchemaVersionInformation(_databaseFolder);
				logWrite("Running from:" + _databaseFolder.Path());

				//If Permission Mode: check if application user name and password/windowsgroup is submitted
				if (_commandLineArgument.PermissionMode)
				{
					if (!(_commandLineArgument.appUserName.Length > 0 && (_commandLineArgument.appUserPwd.Length > 0 || _commandLineArgument.isWindowsGroupName)))
						throw new Exception("No Application user/Windows group name submitted!");
				}

				//Connect and open db
				_sqlConnection = connectAndOpen(_commandLineArgument.ConnectionStringToMaster);

				//Check Azure
				_isAzure = isAzure();

				if (_isAzure && _commandLineArgument.isWindowsGroupName)
					throw new Exception("Windows Azure don't support Windows Login for the moment!");

				//special for Azure => fn_my_permissions does not exist: http://msdn.microsoft.com/en-us/library/windowsazure/ee336248.aspx
				if (_isAzure)
				{
					_isSrvDbCreator = true;
					_isSrvSecurityAdmin = true;
				}
				else
				{
					_isSrvDbCreator = isSrvDbCreator();
					_isSrvSecurityAdmin = isSrvSecurityAdmin();
				}

				//try connect to DB using given AppLogin
				if (!_commandLineArgument.isWindowsGroupName) //SQL
				{
					try
					{
						if (_isAzure)
							_sqlConnectionAppLogin = connectAndOpen(_commandLineArgument.ConnectionStringAppLogOn(_commandLineArgument.DatabaseName));
						else
							_sqlConnectionAppLogin = connectAndOpen(_commandLineArgument.ConnectionStringAppLogOn("master"));

						_loginExist = true;
					}
					catch
					{
						_loginExist = false;
					}
					finally
					{
						if (_sqlConnectionAppLogin != null)
						{
							if (_sqlConnectionAppLogin.State != ConnectionState.Closed)
							{
								_sqlConnectionAppLogin.Close();
							}
						}
					}
				}
				else //Win
					_loginExist = verifyWinGroup(_commandLineArgument.appUserName); //We will need to check on sys.syslogins

				//New installation
				if (_commandLineArgument.WillCreateNewDatabase && !((_isSrvDbCreator && _isSrvSecurityAdmin)))
				{
					throw new Exception("Either sysAdmin or dbCreator + SecurityAdmin permissions are needed to install CCC databases!");
				}

				//patch
				if (!_loginExist && !(_isSrvSecurityAdmin))
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
					SafeMode = false;
					logWrite("Warning: The application will have db_owner permissions. Consider using a different login for the end users!");
					System.Threading.Thread.Sleep(1200);
				}

				_sqlConnection.InfoMessage += sqlConnectionInfoMessage;

				//Exclude Agg from Azure
				if (_isAzure && _commandLineArgument.TargetDatabaseTypeName == DatabaseType.TeleoptiCCCAgg.ToString())

				{
					logWrite("This is a TeleoptiCCCAgg, exclude from SQL Asure");
					return 0;
				}

				if (_commandLineArgument.WillCreateNewDatabase)
				{
					createDB(_commandLineArgument.DatabaseName, _commandLineArgument.TargetDatabaseType);
				}

				//Try create or re-create login
				if (_commandLineArgument.PermissionMode && SafeMode && _isSrvSecurityAdmin)
				{
					createLogin(_commandLineArgument.appUserName, _commandLineArgument.appUserPwd, _commandLineArgument.isWindowsGroupName);
				}

				//Does the db exist?
				if (databaseExists())
				{
					_sqlConnection.Close();      // Close connection to master db.
					_sqlConnection = null;
					_sqlConnection = connectAndOpen(_commandLineArgument.ConnectionString);
					_sqlConnection.InfoMessage += sqlConnectionInfoMessage;

					_databaseVersionInformation = new DatabaseVersionInformation(_databaseFolder, _sqlConnection);

					//if we are not db_owner, bail out
					if (!isDbOwner())
					{
						throw new Exception("db_owner or sysAdmin permissions needed to patch the database!");
					}

					//if this is the very first create, add VersionControl table
					if (_commandLineArgument.WillCreateNewDatabase)
					{
						createDefaultVersionInformation();
					}

					//Set permissions of the newly application user on db.
					if (_commandLineArgument.PermissionMode && SafeMode)
					{
						createPermissions(_commandLineArgument.appUserName);
					}

					//Patch database
					if (_commandLineArgument.PatchMode)
					{
						//Does the Version Table exist?
						if (versionTableExists())
						{
							//Shortcut to release 329, Azure specific script
							if (_isAzure && getDatabaseBuildNumber() == 0)
							{
								applyAzureStartDdl(_commandLineArgument.TargetDatabaseTypeName);
							}

							var dbCreator = new DatabaseSchemaCreator(_databaseVersionInformation,
								_schemaVersionInformation, _sqlConnection, _databaseFolder, Logger);
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

				if (_sqlConnection.State != ConnectionState.Closed)
				{
					_sqlConnection.Close();
				}


				logWrite("Finished !");

				return 0;
			}
         catch (Exception e)
			{
				logWrite("An error occurred: " + e.ToString(), "ERROR");
				return -1;
			}
			finally
			{
				Logger.Dispose();
			}

		}

		private void sqlConnectionInfoMessage(object sender, SqlInfoMessageEventArgs e)
		{
			logWrite(e.Message);
		}

		private void logWrite(string s, string level = "DEBUG")
		{
			Logger.Write(s, level);
		}

		public class MyLogger : IUpgradeLog
		{
			private readonly TextWriter _logFile;

			public MyLogger()
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

		private void applyAzureStartDdl(string databaseTypeName)
		{
			logWrite("Applying Azure DDL starting point...");

			string fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\{1}.00000329.sql",
													  _databaseFolder.Path(), databaseTypeName);

			string sql;
			using (TextReader sqlTextReader = new StreamReader(fileName))
			{
				sql = sqlTextReader.ReadToEnd();
			}

			executeBatchSql(sql);
		}

		private void executeBatchSql(string sql)
		{
			new SqlBatchExecutor(_sqlConnection, Logger).ExecuteBatchSql(sql);
		}

		private int getDatabaseBuildNumber() { return _databaseVersionInformation.GetDatabaseVersion(); }

		private bool versionTableExists()
		{
			string sql = string.Format(CultureInfo.CurrentCulture, "USE [{0}]", _commandLineArgument.DatabaseName);
			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.ExecuteNonQuery();
			}

			sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'DatabaseVersion'";
			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}
		}

		private bool databaseExists()
		{
			const string sql = "SELECT COUNT(*) AS DBExists FROM sys.databases WHERE [name]=@database_name";

			using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@database_name", _commandLineArgument.DatabaseName);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool isAzure()
		{
			const string sql = "IF (SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))) = @azure_edition SELECT 1 ELSE SELECT 0";

			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@azure_edition", AzureEdition);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool isDbOwner()
		{
			const string sql = "select IS_MEMBER ('db_owner')";

			using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool isSrvSecurityAdmin()
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='ALTER ANY LOGIN'";

			using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool isSrvDbCreator()
		{
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='CREATE ANY DATABASE'";

			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool verifyWinGroup(string winNtGroup)
		{
			const string sql = @"SELECT count(name) from sys.syslogins where isntgroup = 1 and name = @WinNTGroup";
			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@WinNTGroup", winNtGroup);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		private  bool dbUserExist(string sqlLogin)
		{
			const string sql = @"select count(*) from sys.sysusers where name = @SQLLogin";
			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@SQLLogin", sqlLogin);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}
		}

		private  bool azureLoginExist(string sqlLogin)
		{
			const string sql = @"select count(*) from sys.sql_logins where name = @SQLLogin";
			using (var sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@SQLLogin", sqlLogin);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}
		}

		private  void createDB(string databaseName, DatabaseType databaseType)
		{
			logWrite("Creating database " + databaseName + "...");

			var creator = new DatabaseCreator(_databaseFolder, _sqlConnection);
			if (_isAzure)
				creator.CreateAzureDatabase(databaseType, databaseName);
			else
				creator.CreateDatabase(databaseType, databaseName);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private  bool sqlVersionGreaterThen(string checkVersion)
		{
			var serverVersion = _sqlConnection.ServerVersion;
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
			else
				return false;
		}

		private  void createLogin(string user, string pwd, Boolean iswingroup)
		{
			//TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
			string sql;
			if (!_isAzure)
			{
				string fileName;
				if (iswingroup)
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", _databaseFolder.Path());
				else
					fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\SQL Logins - Create.sql", _databaseFolder.Path());

				TextReader tr = new StreamReader(fileName);
				sql = tr.ReadToEnd();
				tr.Close();
				sql = sql.Replace("$(SQLLOGIN)", user);
				sql = sql.Replace("$(SQLPWD)", pwd);
				sql = sql.Replace("$(WINLOGIN)", user);
			}
			else
			{
				if (iswingroup)
					sql = "PRINT 'Windows Logins cannot be added to Windows Azure for the momement'";
				else
				{
					if (azureLoginExist(user))
						sql = "ALTER  LOGIN [" + user + "] WITH PASSWORD=N'" + pwd + "'";
					else
						sql = "CREATE LOGIN [" + user + "] WITH PASSWORD=N'" + pwd + "'";
				}
			}

			using (var cmd = new SqlCommand(sql, _sqlConnection))
			{
				cmd.ExecuteNonQuery();
			}

			logWrite("Created login!");

		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		private  void createPermissions(string user)
		{
			string relinkSqlUser;

			//if appication login = sa then don't bother to do anything
			if (compareStringLowerCase(user, string.Format(CultureInfo.CurrentCulture, @"sa")))
				return;

			//Create or Re-link e.g Alter the DB-user from SQL-Login
			var createDBUser = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] FOR LOGIN [{0}]", user);

			if (sqlVersionGreaterThen(SQL2005SP2))
				relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"ALTER USER [{0}] WITH LOGIN = [{0}]", user);
			else
				relinkSqlUser = string.Format(CultureInfo.CurrentCulture, @"sp_change_users_login 'Update_One', '{0}', '{0}'", user);

			if (dbUserExist(user))
			{
				logWrite("DB user already exist, re-link ...");
				using (var cmd = new SqlCommand(relinkSqlUser, _sqlConnection))
					cmd.ExecuteNonQuery();
			}
			else
			{
				logWrite("DB user is missing. Create DB user ...");
				using (var cmd = new SqlCommand(createDBUser, _sqlConnection))
					cmd.ExecuteNonQuery();
			}

			//Add permission
			var fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", _databaseFolder.Path());

			var sql = File.ReadAllText(fileName);

			sql = sql.Replace("$(LOGIN)", user);

			using (var cmd = new SqlCommand(sql, _sqlConnection))
				cmd.ExecuteNonQuery();

			logWrite("Created Permissions!");
		}

		private  void createDefaultVersionInformation()
		{
			logWrite("Creating database version table and setting inital version to 0...");
			_databaseVersionInformation.CreateTable();
		}

		private  SqlConnection connectAndOpen(string connectionString)
		{
			SqlConnection sqlConnection = new SqlConnection(connectionString);
			sqlConnection.Open();
			return sqlConnection;
		}

		 private bool compareStringLowerCase(string stringA, string stringB)
		{
			return (string.Compare(stringA.ToLower(CultureInfo.CurrentCulture), stringB.ToLower(CultureInfo.CurrentCulture), true,
				 System.Globalization.CultureInfo.CurrentCulture) == 0);
		}
	}
}