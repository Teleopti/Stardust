using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Teleopti.Ccc.DBManager.Library;

namespace Teleopti.Ccc.DBManager
{
    class Program
    {
        private static SqlConnection _sqlConnection;
		private static SqlConnection _sqlConnectionAppLogin;
        private static DatabaseFolder _databaseFolder;
        private static CommandLineArgument _commandLineArgument;

        //move to security object or something
        private static bool _isAzure;
        private static bool _isSrvDbCreator;
        private static bool _isSrvSecurityAdmin;
		private static bool _loginExist;

        private const string AzureEdition = "SQL Azure";
		private const string SQL2005SP2 = "9.00.3042"; //http://www.sqlteam.com/article/sql-server-versions
    	private static MyLogger _logger;
		private static DatabaseVersionInformation _databaseVersionInformation;
		private static SchemaVersionInformation _schemaVersionInformation;

    	/// <summary>
        /// Mains the specified args.
        /// Usage:
        /// DBManager.exe (-c) [*Database name] [Login name] [Password] [Server name]
        /// -c switch creates a database with given name.
        /// Database name is required.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-08-15
        /// </remarks>
        static int Main(string[] args)
        {
            try
            {
				_logger = new MyLogger();

                // hhmmss
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string versionNumber = version.ToString();
                bool SafeMode = true;
                
                logWrite("Teleopti Database Manager version " + versionNumber);

                if (args.Length > 0 && args.Length < 20)
                {
                    _commandLineArgument = new CommandLineArgument(args);
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
                    _sqlConnection = ConnectAndOpen(_commandLineArgument.ConnectionStringToMaster);

					//Check Azure
					_isAzure = IsAzure();

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
						_isSrvDbCreator = IsSrvDbCreator();
						_isSrvSecurityAdmin = IsSrvSecurityAdmin();
					}

					//try connect to DB using given AppLogin
					if (!_commandLineArgument.isWindowsGroupName) //SQL
					{
						try
						{
							if (_isAzure)
								_sqlConnectionAppLogin = ConnectAndOpen(_commandLineArgument.ConnectionStringAppLogOn(_commandLineArgument.DatabaseName));
							else
								_sqlConnectionAppLogin = ConnectAndOpen(_commandLineArgument.ConnectionStringAppLogOn("master"));

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
						_loginExist = VerifyWinGroup(_commandLineArgument.appUserName); //We will need to check on sys.syslogins

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
						(CompareStringLowerCase(_commandLineArgument.appUserName, _commandLineArgument.UserName))
                        )
                    {
                        SafeMode = false;
                        logWrite("Warning: The application will have db_owner permissions. Consider using a different login for the end users!");
                        System.Threading.Thread.Sleep(1200);
                    }

                    _sqlConnection.InfoMessage += _sqlConnection_InfoMessage;

                    //Exclude Agg from Azure
                    if (_isAzure && _commandLineArgument.TargetDatabaseTypeName == DatabaseType.TeleoptiCCCAgg.ToString())

                    {
                        logWrite("This is a TeleoptiCCCAgg, exclude from SQL Asure");
                        return 0;
                    }
                    
                    if (_commandLineArgument.WillCreateNewDatabase)
                    {
                        CreateDB(_commandLineArgument.DatabaseName, _commandLineArgument.TargetDatabaseType);
                    }

                    //Try create or re-create login
					if (_commandLineArgument.PermissionMode && SafeMode && _isSrvSecurityAdmin)
                    {
                        CreateLogin(_commandLineArgument.appUserName, _commandLineArgument.appUserPwd, _commandLineArgument.isWindowsGroupName);
                    }

                    //Does the db exist?
                    if (DatabaseExists())
                    {
                        _sqlConnection.Close();      // Close connection to master db.
                        _sqlConnection = null;
                        _sqlConnection = ConnectAndOpen(_commandLineArgument.ConnectionString);
                        _sqlConnection.InfoMessage += _sqlConnection_InfoMessage;

						_databaseVersionInformation = new DatabaseVersionInformation(_databaseFolder, _sqlConnection);

                        //if we are not db_owner, bail out
                        if (!IsDbOwner())
                        {
                            throw new Exception("db_owner or sysAdmin permissions needed to patch the database!");
                        }

                        //if this is the very first create, add VersionControl table
                        if (_commandLineArgument.WillCreateNewDatabase)
                        {
                            CreateDefaultVersionInformation();
                        }

                        //Set permissions of the newly application user on db.
                        if (_commandLineArgument.PermissionMode && SafeMode)
                        {
                            CreatePermissions(_commandLineArgument.appUserName);
                        }

                        //Patch database
                        if (_commandLineArgument.PatchMode)
                        {
                            //Does the Version Table exist?
                            if (VersionTableExists())
                            {
                                //Shortcut to release 329, Azure specific script
                                if (_isAzure && GetDatabaseBuildNumber() == 0)
                                {
                                    applyAzureStartDDL(_commandLineArgument.TargetDatabaseTypeName);
                                }

                                //Add Released DDL
                                applyReleases(_commandLineArgument.TargetDatabaseType);
                                //Add upcoming DDL (Trunk)
                                applyTrunk(_commandLineArgument.TargetDatabaseType);
                                //Add Programmabilty
                                applyProgrammability(_commandLineArgument.TargetDatabaseType);
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
                }
                else
                {
                    Console.Out.WriteLine("Usage:");
                    Console.Out.WriteLine("DBManager.exe [switches]");
                    Console.Out.WriteLine("-S[Server name]");
                    Console.Out.WriteLine("-D[Database name]");
                    //Console.Out.WriteLine("-I[Schema name] if omitted dbo will be used");
                    Console.Out.WriteLine("-U[User name]");
                    Console.Out.WriteLine("-P[Password]");
                    Console.Out.WriteLine("-N[Target build number]");
                    Console.Out.WriteLine("-E Uses Integrated Security, otherwise SQL Server security.");
                    string databaseTypeList = string.Join("|", Enum.GetNames(typeof(DatabaseType)));
                    Console.Out.WriteLine(string.Format(CultureInfo.CurrentCulture, "-O[{0}]", databaseTypeList));
                    Console.Out.WriteLine("-C Creates a database with name from -D switch.");
                    Console.Out.WriteLine("-L[sqlUserName:sqlUserPassword] Will create a sql user that the application will use when running. Mandatory while using -C or -R");
                    Console.Out.WriteLine("-W[Local windows Group] Will create a Windows Group that the application will use when running. . Mandatory while using -C or -R");
                    Console.Out.WriteLine("-B[Business Unit]");
                    Console.Out.WriteLine("-F Path to where dbmanager runs from");
                }
                return 0;
            }
            catch (Exception e)
            {
                logWrite("An error occurred:");
                logWrite(e.Message);
                return -1;
            }
            finally
            {
                _logger.Dispose();
            }
        }

        static void _sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            logWrite(e.Message);
        }

        private static void logWrite(string s)
        {
        	_logger.Write(s);
        }

		public class MyLogger : ILog, IDisposable
		{
			private readonly TextWriter _logFile;

			public MyLogger()
			{
				var nowDateTime = DateTime.Now;
				_logFile = new StreamWriter(string.Format(CultureInfo.CurrentCulture, "DBManager_{0}_{1}.log", nowDateTime.ToString("yyyyMMdd", CultureInfo.CurrentCulture), nowDateTime.ToString("hhmmss", CultureInfo.CurrentCulture)));
			}

			public void Write(string message)
			{
				Console.Out.WriteLine(message);
				_logFile.WriteLine(message);
			}

			public void Dispose()
			{
				if (_logFile == null)
					return;
				_logFile.Close();
				_logFile.Dispose();
			}
		}

        private static void applyReleases(DatabaseType databaseType)
        {
			new DatabaseSchemaCreator(_databaseVersionInformation, _schemaVersionInformation, _sqlConnection, _databaseFolder, _logger)
				.ApplyReleases(databaseType);
        }

        private static void applyAzureStartDDL(string databaseTypeName)
        {
            logWrite("Applying Azure DDL starting point...");

            string fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\{1}.00000329.sql",
                                            _databaseFolder.Path(), databaseTypeName);

            string sql;
            using(TextReader sqlTextReader = new StreamReader(fileName))
            {
                sql = sqlTextReader.ReadToEnd();
            }

            executeBatchSQL(sql);
        }

    	private static void executeBatchSQL(string sql)
    	{
			new SqlBatchExecutor(_sqlConnection,_logger).ExecuteBatchSql(sql);
    	}

		private static void applyTrunk(DatabaseType databaseType)
        {
			new DatabaseSchemaCreator(_databaseVersionInformation, _schemaVersionInformation, _sqlConnection, _databaseFolder, _logger)
				.ApplyTrunk(databaseType);
        }

		private static void applyProgrammability(DatabaseType databaseType)
		{
			new DatabaseSchemaCreator(_databaseVersionInformation, _schemaVersionInformation, _sqlConnection, _databaseFolder, _logger)
				.ApplyProgrammability(databaseType);
		}

        private static int GetDatabaseBuildNumber() { return _databaseVersionInformation.GetDatabaseVersion(); }

        private static bool VersionTableExists()
        {
            string sql = string.Format(CultureInfo.CurrentCulture, "USE [{0}]", _commandLineArgument.DatabaseName);
            using(var sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.ExecuteNonQuery();
            }

            sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME LIKE 'DatabaseVersion'";
            using(var sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
            }
        }

        private static bool DatabaseExists()
        {
            const string sql = "SELECT COUNT(*) AS DBExists FROM sys.databases WHERE [name]=@database_name";

            using(SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@database_name", _commandLineArgument.DatabaseName);
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static bool IsAzure()
        {
            const string sql = "IF (SELECT CONVERT(NVARCHAR(200), SERVERPROPERTY('edition'))) = @azure_edition SELECT 1 ELSE SELECT 0";

            using(SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@azure_edition", AzureEdition);
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static bool IsDbOwner()
        {
            const string sql = "select IS_MEMBER ('db_owner')";

            using(SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static bool IsSrvSecurityAdmin()
        {
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='ALTER ANY LOGIN'";

            using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static bool IsSrvDbCreator()
        {
			const string sql = "SELECT count(*) FROM fn_my_permissions(NULL, 'SERVER') WHERE permission_name='CREATE ANY DATABASE'";

            using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

		private static bool VerifyWinGroup(string WinNTGroup)
		{
			const string sql = @"SELECT count(name) from sys.syslogins where isntgroup = 1 and name = @WinNTGroup";
			using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@WinNTGroup", WinNTGroup);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

        private static bool DBUserExist(string SQLLogin)
        {
            const string sql = @"select count(*) from sys.sysusers where name = @SQLLogin";
            using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
            {
                sqlCommand.Parameters.AddWithValue("@SQLLogin", SQLLogin);
                return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
            }
        }

		private static bool AzureLoginExist(string SQLLogin)
		{
			const string sql = @"select count(*) from sys.sql_logins where name = @SQLLogin";
			using (SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
			{
				sqlCommand.Parameters.AddWithValue("@SQLLogin", SQLLogin);
				return Convert.ToBoolean(sqlCommand.ExecuteScalar(), CultureInfo.CurrentCulture);
			}
		}

        private static void CreateDB(string databaseName, DatabaseType databaseType)
        {
            logWrite("Creating database " + databaseName + "...");

        	var creator = new DatabaseCreator(_databaseFolder, _sqlConnection);
			if (_isAzure)
				creator.CreateAzureDatabase(databaseType, databaseName);
			else
				creator.CreateDatabase(databaseType, databaseName);
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		private static bool SQLVersionGreaterThen(string checkVersion)
		{
			string serverVersion = _sqlConnection.ServerVersion;
			string[] serverVersionDetails = serverVersion.Split(new string[] { "." }, StringSplitOptions.None);
			string[] checkVersionDetails = checkVersion.Split(new string[] { "." }, StringSplitOptions.None);

			if (checkVersionDetails.Length < 3 && serverVersionDetails.Length < 3)
				throw new Exception("Unknown version string given from SQL Server or in code");

				int majorVersionNumber = int.Parse(serverVersionDetails[0], CultureInfo.InvariantCulture);
				int minorVersionNumber = int.Parse(serverVersionDetails[1], CultureInfo.InvariantCulture);
				int buildVersionNumber = int.Parse(serverVersionDetails[2], CultureInfo.InvariantCulture);

				int majorCheckVersionNumber = int.Parse(checkVersionDetails[0], CultureInfo.InvariantCulture);
				int minorCheckVersionNumber = int.Parse(checkVersionDetails[1], CultureInfo.InvariantCulture);
				int buildCheckVersionNumber = int.Parse(checkVersionDetails[2], CultureInfo.InvariantCulture);

				if (majorCheckVersionNumber < majorVersionNumber)
					return true;
				if (minorCheckVersionNumber < minorVersionNumber)
					return true;
				if (buildCheckVersionNumber < buildVersionNumber)
					return true;
				else
					return false;
		}

        private static void CreateLogin(string user, string pwd, Boolean iswingroup)
        {
            //TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
            string fileName = "";
			string sql = "";
			if (!_isAzure)
			{
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
					if (AzureLoginExist(user) == true)
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
        private static void CreatePermissions(string user)
        {
            string fileName;
            string sql;
            string relinkSQLUser;
            string createDBUser;

            //if appication login = sa then don't bother to do anything
            if (CompareStringLowerCase(user,string.Format(CultureInfo.CurrentCulture, @"sa")))
                return;
			
			//Create or Re-link e.g Alter the DB-user from SQL-Login
			createDBUser = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] FOR LOGIN [{0}]", user);

			if (SQLVersionGreaterThen(SQL2005SP2))
				relinkSQLUser = string.Format(CultureInfo.CurrentCulture, @"ALTER USER [{0}] WITH LOGIN = [{0}]", user);
			else
				relinkSQLUser = string.Format(CultureInfo.CurrentCulture, @"sp_change_users_login 'Update_One', '{0}', '{0}'", user);

			if (DBUserExist(user))
			{
				logWrite("DB user already exist, re-link ...");
				using (var cmd = new SqlCommand(relinkSQLUser, _sqlConnection))
				cmd.ExecuteNonQuery();
			}
			else
			{
				logWrite("DB user is missing. Create DB user ...");
				using (var cmd = new SqlCommand(createDBUser, _sqlConnection))
				cmd.ExecuteNonQuery();
			}

			//Add permission
            fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", _databaseFolder.Path());

            sql = File.ReadAllText(fileName);

            sql = sql.Replace("$(LOGIN)", user);

			using (var cmd = new SqlCommand(sql, _sqlConnection))
				cmd.ExecuteNonQuery();

            logWrite("Created Permissions!");
        }

        private static void CreateDefaultVersionInformation()
        {
        	logWrite("Creating database version table and setting inital version to 0...");
			_databaseVersionInformation.CreateTable();
        }

    	private static SqlConnection ConnectAndOpen(string connectionString)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }

		static bool CompareStringLowerCase(string stringA, string stringB)
		{
			return (string.Compare(stringA.ToLower(CultureInfo.CurrentCulture), stringB.ToLower(CultureInfo.CurrentCulture), true,
				 System.Globalization.CultureInfo.CurrentCulture) == 0);
		}
    }
}