﻿using System;
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
        private static DatabaseFolder _databaseFolder;
        private static CommandLineArgument _commandLineArgument;
        private static bool _isAzure;
        private const string AzureEdition = "SQL Azure";
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
                
                logWrite("Teleopti Database Manager version " + versionNumber);

                if (args.Length > 0 && args.Length < 10)
                {
                    _commandLineArgument = new CommandLineArgument(args);
#if DEBUG
					logWrite("DBManager is running in DEBUG Mode...");
#endif
					_databaseFolder = new DatabaseFolder(new DbManagerFolder(_commandLineArgument.PathToDbManager));
					_schemaVersionInformation = new SchemaVersionInformation(_databaseFolder);
                    logWrite("Running from:" + _databaseFolder.Path());

                    //Connect and open db
                    _sqlConnection = ConnectAndOpen(_commandLineArgument.ConnectionStringToMaster);

                    //Check Azure
                    _isAzure = IsAzure();

                    if (_isAzure && _commandLineArgument.isWindowsGroupName)
                        throw new Exception("Windows Azure don't support Windows Login for the moment!");

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

                    if (_commandLineArgument.PermissionMode)
                    {
                        //check if application user name and password/windowsgroup is submitted
                        if (_commandLineArgument.appUserName.Length > 0 && (_commandLineArgument.appUserPwd.Length > 0 || _commandLineArgument.isWindowsGroupName))
                        {
                            CreateLogin(_commandLineArgument.appUserName, _commandLineArgument.appUserPwd, _commandLineArgument.isWindowsGroupName);
                        }
                        else
                        {
                            throw new Exception("No Application user/Windows group name submitted!");
                        }

                    }

                    //Does the db exist?
                    if (DatabaseExists())
                    {
                        _sqlConnection.Close();      // Close connection to master db.
                        _sqlConnection = null;
                        _sqlConnection = ConnectAndOpen(_commandLineArgument.ConnectionString);
                        _sqlConnection.InfoMessage += _sqlConnection_InfoMessage;

						_databaseVersionInformation = new DatabaseVersionInformation(_databaseFolder, _sqlConnection);

                        //if this is the very first create, add VersionControl table
                        if (_commandLineArgument.WillCreateNewDatabase)
                        {
                            CreateDefaultVersionInformation();
                        }

                        //Set permissions of the newly application user on db.
                        if (_commandLineArgument.PermissionMode)
                        {
                            //check if application user name and password/windowsgroup is submitted
                            if (_commandLineArgument.appUserName.Length > 0 && (_commandLineArgument.appUserPwd.Length > 0 || _commandLineArgument.isWindowsGroupName))
                            {
                                CreatePermissions(_commandLineArgument.appUserName, _commandLineArgument.isWindowsGroupName);
                            }
                            else
                            {
                                throw new Exception("No Application user/Windows group name submitted!");
                            }

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
                    Console.Out.WriteLine("-L[sqlUserName:sqlUserPassword] Will create a sql user that the application will use when running. Mandatory while using -C if -W is not used");
                    Console.Out.WriteLine("-W[Local windows Group] Will create a Windows Group that the application will use when running. . Mandatory while using -C if -L is not used");
                    Console.Out.WriteLine("-B[Business Unit]");
                    Console.Out.WriteLine("-T Add the trunk (leave this out if you don't want the trunk.)");
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

        private static bool SQLLoginExist(string SQLLogin)
        {
            const string sql = @"select count(*) from sys.sql_logins where name = @SQLLogin";

            using(SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection))
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

        private static void CreateLogin(string user, string pwd, Boolean iswingroup)
        {
            //TODO: check if windows group and run win logon script instead of "SQL Logins - Create.sql"
            string fileName = "";
            if (_isAzure)
            {
                if (iswingroup)
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\Win Logins - Create.sql", _databaseFolder.Path());
                else
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\SQL Logins - Create.sql", _databaseFolder.Path());
            }
            else
            {
                if (iswingroup)
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", _databaseFolder.Path());
                else
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\SQL Logins - Create.sql", _databaseFolder.Path());
            }
    
            TextReader tr = new StreamReader(fileName);
            string sql = tr.ReadToEnd();
            tr.Close();
            sql = sql.Replace("$(SQLLOGIN)", user);
            sql = sql.Replace("$(SQLPWD)", pwd);
            sql = sql.Replace("$(WINLOGIN)", user);

            using (var cmd = new SqlCommand(sql, _sqlConnection))
            {
                if (_isAzure && SQLLoginExist(user) == true)
                {
                    logWrite("Azure Login already exist, continue ...");
                    
                }
                else
                    cmd.ExecuteNonQuery();
            }

            logWrite("Created login!");

        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private static void CreatePermissions(string user, bool iswingroup)
        {
            string fileName;
            string sql;
            sql = "";
            if (_isAzure && !iswingroup)
            {
                sql = string.Format(CultureInfo.CurrentCulture, @"CREATE USER [{0}] FOR LOGIN [{0}]", user);
                using (var cmd = new SqlCommand(sql, _sqlConnection))
                {
                    cmd.ExecuteNonQuery();
                }

                fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\permissions - add.sql", _databaseFolder.Path());
            }
            else
            {
                fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", _databaseFolder.Path());
            }              

            sql = System.IO.File.ReadAllText(fileName);

            sql = sql.Replace("$(LOGIN)", user);

            if (iswingroup)
            {
                sql = sql.Replace("$(AUTHTYPE)", "WIN");
            }
            else
            {
                sql = sql.Replace("$(AUTHTYPE)", "SQL");
            }

            using (var cmd = new SqlCommand(sql, _sqlConnection))
            {
                cmd.ExecuteNonQuery();
            }

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
    }
}