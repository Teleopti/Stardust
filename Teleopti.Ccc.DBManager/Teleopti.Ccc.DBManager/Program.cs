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
        private static TextWriter _logger;
        private static int _currentDatabaseBuildNumber;
        private static SqlConnection _sqlConnection;
        private static string _baseDirectory;
        private static CommandLineArgument _commandLineArgument;
        private static string _fileName;
        private const int CommandTimeout = 900; //Command Timeout for Create + Release scripts
        private static bool _isAzure;
        private const string AzureEdition = "SQL Azure";
        
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
                DateTime nowDateTime = DateTime.Now;

                _logger = new StreamWriter(string.Format(CultureInfo.CurrentCulture, "DBManager_{0}_{1}.log", nowDateTime.ToString("yyyyMMdd", CultureInfo.CurrentCulture), nowDateTime.ToString("hhmmss", CultureInfo.CurrentCulture)));
                // hhmmss
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                string versionNumber = version.ToString();
                
                logWrite("Teleopti Database Manager version " + versionNumber);

                if (args.Length > 0 && args.Length < 9)
                {
                    _commandLineArgument = new CommandLineArgument(args);

#if DEBUG
                    logWrite("DBManager is running in DEBUG Mode...");
                    _baseDirectory = @"..\..\..\..\Database";
#else
                    _baseDirectory = _commandLineArgument.PathToDbManager;
                    if (_baseDirectory.Equals(string.Empty))
                        _baseDirectory = getBaseDirectory();
#endif
                    _baseDirectory = Path.GetFullPath(_baseDirectory);
                    logWrite("Running from:" + _baseDirectory);

                    //Connect and open db
                    _sqlConnection = ConnectAndOpen(_commandLineArgument.ConnectionStringToMaster);

                    //Check Azure
                    _isAzure = IsAzure();

                    _sqlConnection.InfoMessage += _sqlConnection_InfoMessage;

                    //Exclude Agg from Azure
                    if (_isAzure && _commandLineArgument.TargetDatabaseTypeName == DatabaseType.TeleoptiCCCAgg.ToString())

                    {
                        logWrite("This is a TeleoptiCCCAgg, exclude from SQL Asure");
                        return 0;
                    }
                    
                    if (_commandLineArgument.WillCreateNewDatabase)
                    {
                        //check if application user name and password/windowsgroup is submitted
                        if (_commandLineArgument.appUserName.Length > 0 && (_commandLineArgument.appUserPwd.Length > 0 || _commandLineArgument.isWindowsGroupName))
                        {

                            CreateDB(_commandLineArgument.DatabaseName, _commandLineArgument.TargetDatabaseType);
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

                        //Set permissions of the newly application user on db.
                        if (_commandLineArgument.WillCreateNewDatabase)
                        {
                            CreateDefaultVersionInformation();
                            CreatePermissions(_commandLineArgument.appUserName, _commandLineArgument.isWindowsGroupName);
                        }

                        //Does the Version Table exist?
                        if (VersionTableExists())
                        {
                            //Shortcut to release 329, Azure specific script
                            if (_isAzure && GetDatabaseBuildNumber() == 0)
                            {
                                applyAzureStartDDL(_commandLineArgument.TargetDatabaseTypeName);
                            }

                            //Add Released DDL
                            applyReleases(_commandLineArgument.TargetDatabaseTypeName);
                            //Add upcoming DDL (Trunk)
                            if (_commandLineArgument.WillAddTrunk)
                                applyTrunk(_commandLineArgument.TargetDatabaseTypeName);
                            //Add Programmabilty
                            applyProgrammability(_commandLineArgument.TargetDatabaseTypeName);
                        }
                        else
                        {
                            logWrite("Version information is missing, is this a Raptor db?");
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
                if (_fileName != "")
                {
                    logWrite("Last opened script file: ");
                    logWrite(_fileName);
                }
                logWrite(e.Message);
                return -1;
            }
            finally
            {
                _logger.Close();

            }
        }

        private static string getBaseDirectory()
        {

            string baseDirectory = string.Empty;
            Assembly[] assemblyCollection = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblyCollection)
            {
                if (assembly.ManifestModule.Name.ToLower() == "dbmanager.exe")
                {
                    baseDirectory = Path.GetDirectoryName(assembly.Location);
                }
            }
            return baseDirectory;
        }

        static void _sqlConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            logWrite(e.Message);
        }

        private static void logWrite(string s)
        {
            Console.Out.WriteLine(s);
            _logger.WriteLine(s);
        }

        /// <summary>
        /// Applies the releases.
        /// </summary>
        /// <param name="databaseTypeName">Name of the database type.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-09-23
        /// </remarks>
        private static void applyReleases(string databaseTypeName)
        {
            _currentDatabaseBuildNumber = GetDatabaseBuildNumber();
            string releasesPath = _baseDirectory + @"\" + databaseTypeName + @"\Releases";
            if (Directory.Exists(releasesPath))
            {
                DirectoryInfo scriptsDirectoryInfo = new DirectoryInfo(releasesPath);
                FileInfo[] scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

                foreach (FileInfo scriptFile in scriptFiles)
                {
                    //Check the versionnumber
                    string releaseName = scriptFile.Name.Replace(".sql", "");
                    int releaseNumber = Convert.ToInt32(releaseName, CultureInfo.CurrentCulture);

                    //Always add release files (DDL and src-code)
                    if (releaseNumber > _currentDatabaseBuildNumber)
                    {
                        logWrite("Applying Release " + releaseName + "...");
                        _fileName = scriptFile.FullName;
                        string sql;
                        using (TextReader textReader = new StreamReader(_fileName))
                        {
                            sql = textReader.ReadToEnd();
                        }
                        executeBatchSQL(sql);

                        //Don't add data If AddDataFiles is false. Used by Nightly builds
                        if (_commandLineArgument.AddDatFiles)
                            executeDatFile(scriptFile);
                    }
                }
            }
        }

        private static void executeDatFile(FileSystemInfo scriptFile)
        {
            _fileName = scriptFile.FullName.Replace(".sql", ".dat");
            try
            {
                using (TextReader datReader = new StreamReader(_fileName))
                {
                    string datSql = datReader.ReadToEnd();
                    executeBatchSQL(datSql);
                }
            }
            catch (IOException)
            {
                //logWrite("Cannot find file " + datFile);
            }
        }


        private static void executeBatchSQL(string sql)
        {
            SqlTransaction transaction = _sqlConnection.BeginTransaction();
            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(sql);

            using (SqlCommand sqlCommand = _sqlConnection.CreateCommand())
            {
                sqlCommand.Connection = _sqlConnection;
                sqlCommand.CommandTimeout = CommandTimeout;
                sqlCommand.Transaction = transaction;

                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        sqlCommand.CommandText = line;
                        sqlCommand.CommandType = CommandType.Text;

                        try
                        {
                            sqlCommand.ExecuteNonQuery();
                        }
                        catch (SqlException e)
                        {
                            logWrite(e.Message);
                            transaction.Rollback();
                            break;
                            //In this case stop the whole thing
                        }
                    }
                }
                transaction.Commit();
            }
        }

        private static void applyAzureStartDDL(string databaseTypeName)
        {
            logWrite("Applying Azure DDL starting point...");

            string fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\{1}.00000329.sql",
                                            _baseDirectory, databaseTypeName);

            string sql;
            using(TextReader sqlTextReader = new StreamReader(fileName))
            {
                sql = sqlTextReader.ReadToEnd();
            }
            executeBatchSQL(sql);
        }


        private static void applyTrunk(string databaseTypeName)
        {
            logWrite("Applying Trunk...");
            string trunkPath = _baseDirectory + @"\" + databaseTypeName + @"\Trunk";
            if (Directory.Exists(trunkPath))
            {
                string sqlFile = string.Format(CultureInfo.CurrentCulture, @"{0}\Trunk.sql", trunkPath);
                string datFile = string.Format(CultureInfo.CurrentCulture, @"{0}\Trunk.dat", trunkPath);

                string sql;
                using (TextReader sqlTextReader = new StreamReader(sqlFile))
                {
                    sql = sqlTextReader.ReadToEnd();
                }

                //Always DDL and src-code
                executeBatchSQL(sql);

                //Add data If AddDataFiles is true. Used by Nightly builds (-A)
                if (_commandLineArgument.AddDatFiles)
                {
                    if (File.Exists(datFile))
                    {
                        string dat;
                        using (TextReader datTextReader = new StreamReader(datFile))
                        {
                            dat = datTextReader.ReadToEnd();
                        }
                        executeBatchSQL(dat);
                    }
                }
            }
        }


        private static void applyProgrammability(string databaseTypeName)
        {
            string progPath = _baseDirectory + @"\" + databaseTypeName + @"\Programmability";
            string[] directories = Directory.GetDirectories(progPath);

            foreach (string directory in directories)
            {
                if (Directory.Exists(directory))
                {
                    DirectoryInfo scriptsDirectoryInfo = new DirectoryInfo(directory);
                    FileInfo[] scriptFiles = scriptsDirectoryInfo.GetFiles("*.sql", SearchOption.TopDirectoryOnly);

                    logWrite(string.Format("Adding programmability directory '{0}'", scriptsDirectoryInfo.Name));

                    foreach (FileInfo scriptFile in scriptFiles)
                    {
                        _fileName = scriptFile.FullName;
                        string sql;
                        using(TextReader textReader = new StreamReader(_fileName))
                        {
                            sql = textReader.ReadToEnd();
                        }
                        executeBatchSQL(sql);
                    }
                }
            }
        }

        private static int GetDatabaseBuildNumber()
        {
            using(SqlCommand sqlCommand = new SqlCommand("SELECT MAX(BuildNumber) FROM dbo.[DatabaseVersion]", _sqlConnection))
            {
                return (int)sqlCommand.ExecuteScalar();
            }
        }

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

        	var creator = new DatabaseCreator(_baseDirectory, _sqlConnection);
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
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", _baseDirectory);
                else
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\SQL Logins - Create.sql", _baseDirectory);
            }
            else
            {
                if (iswingroup)
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Win Logins - Create.sql", _baseDirectory);
                else
                    fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\SQL Logins - Create.sql", _baseDirectory);
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

                fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\Azure\permissions - add.sql", _baseDirectory);
            }
            else
            {
                fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\permissions - add.sql", _baseDirectory);
            }              

            sql = System.IO.File.ReadAllText(fileName);

            if (iswingroup)
            {
                sql = sql.Replace("$(SVCLOGIN)", user);
                sql = sql.Replace("$(AUTHTYPE)", "WIN");
            }
            else
            {
                sql = sql.Replace("$(SQLLOGIN)", user);
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
            string fileName = string.Format(CultureInfo.CurrentCulture, @"{0}\Create\CreateDatabaseVersion.sql", _baseDirectory);
            TextReader textReader = new StreamReader(fileName);
            string sql = textReader.ReadToEnd();
            textReader.Close();
            SqlCommand sqlCommand = new SqlCommand(sql, _sqlConnection);
            sqlCommand.ExecuteNonQuery();
        }

        private static SqlConnection ConnectAndOpen(string connectionString)
        {
            SqlConnection sqlConnection = new SqlConnection(connectionString);
            sqlConnection.Open();
            return sqlConnection;
        }
    }
}