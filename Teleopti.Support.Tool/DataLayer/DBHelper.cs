using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;


namespace  Teleopti.Support.Tool.DataLayer
{

    public enum Right
    {
        DBOwner,
        Limited,
        None,
        SA
    }

    public class DBHelper
    {
        private readonly string _connString;
        private readonly string _serverName;

        /// <summary>
        /// Creates a new connection to the server
        /// </summary>
        /// <param name="server">Name of the server</param>
        /// <param name="user">User name to login with</param>
        /// <param name="password">user password</param>
        public DBHelper(string server, string user, string password)
        {
            _serverName = server;
            _connString = "Server=" + server + ";Database=Master;User=" + user + ";Password=" + password + ";";
        }

        /// <summary>
        /// Creates a new, trusted, connection to the server
        /// </summary>
        /// <param name="server">Name of the server</param>
        public DBHelper(string server,string database)
        {
            _serverName = server;
            _connString = "Server=" + server + ";Database=" + database +";Trusted_Connection=True;";
        }

        public DBHelper(string connectionString)
        {
            _connString = connectionString;
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(_connString);
            _serverName = sqlConnectionStringBuilder.DataSource;

        }

        public string ServerName
        {
            get { return _serverName; }
        }

        public string GetDatabaseVersion()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            return GetDatabaseVersion(builder.InitialCatalog);
        }

        /// <summary>
        /// Get what database version this database is on
        /// </summary>
        /// <param name="database">The databse name</param>
        /// <returns>The databse version</returns>
        public string GetDatabaseVersion(String database)
        {
            using (DataSet ds = Execute("select MAX(SystemVersion) from DatabaseVersion where SystemVersion <> 'Not defined'", database))
            {
                return Convert.ToString(ds.Tables[0].Rows[0].ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

	    public Dictionary<string, string> GetServerConfigurations()
	    {
			var result = new Dictionary<string, string>();
			var builder = new SqlConnectionStringBuilder(ConnectionString);
			using (var ds = Execute("select [Key], [Value] from Tenant.ServerConfiguration", builder.InitialCatalog))
			{
				foreach (DataTable table in ds.Tables)
				{
					foreach (DataRow row in table.Rows)
					{
						var key = Convert.ToString(row.ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
						var value = Convert.ToString(row.ItemArray[1], System.Globalization.CultureInfo.InvariantCulture);
						result.Add(key, value);
					}
				}
			}
			return result;
		}

		public int GetDatabaseBuildNumber()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            return GetDatabaseBuildNumber(builder.InitialCatalog);
        }

        /// <summary>
        /// Get what database build number this database is on
        /// </summary>
        /// <param name="database">The database name</param>
        /// <returns>The database build number</returns>
        private int GetDatabaseBuildNumber(String database)
        {
            using (DataSet ds = Execute("select MAX(BuildNumber) from DatabaseVersion where SystemVersion <> 'Not defined'", database))
            {
                int buildNumber = Convert.ToInt32(ds.Tables[0].Rows[0].ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
                return buildNumber;
            }
        }

        public string GetAggDatabaseName()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
            return GetAggDatabaseName(builder.InitialCatalog);
        }

       /// <summary>
       /// Get the what aggregation database this analytic databse is connected to
       /// </summary>
       /// <param name="analyticDatabase"></param>
       /// <returns></returns>
        public string GetAggDatabaseName(String analyticDatabase)
        {
            using (DataSet ds = Execute("SELECT target_customname from mart.sys_crossdatabaseview_target", analyticDatabase))
            {
                return Convert.ToString(ds.Tables[0].Rows[0].ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Checkes if the users that is connected to the sql server have the Database Role role on the database db 
        /// </summary>
        /// <param name="db">The databse to check on</param>
        /// <param name="role">The role</param>
        /// <returns>returns true if the user has the role oon the database, otherwise false</returns>
        public bool IsDBMember(String db, String role)
        {
            using (DataSet ds = Execute("SELECT IS_MEMBER('" + role + "')", db))
            {
                return Convert.ToBoolean(ds.Tables[0].Rows[0].ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Checkes if the users that is connected to the sql server have the Server Role role on the database db 
        /// </summary>
        /// <param name="db">The databse to check on</param>
        /// <param name="role">The role</param>
        /// <returns>Returns true if the user has the role on the server, otherwise false</returns>
        public bool IsSRMember(String db, String role)
        {
            using (DataSet ds = Execute("SELECT IS_SRVROLEMEMBER('" + role + "')", db))
            {
                return Convert.ToBoolean(ds.Tables[0].Rows[0].ItemArray[0], System.Globalization.CultureInfo.InvariantCulture);
            }
        }


        public bool TestConnection()
        {
            bool connectionIsOk = true;
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(ConnectionString);
                builder.ConnectTimeout = 10;
                SqlConnection sqlConnection = new SqlConnection(builder.ConnectionString);
                sqlConnection.Open();
                SqlCommand sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandTimeout = 10;
                sqlCommand.CommandText = "select MAX(SystemVersion) from DatabaseVersion where SystemVersion <> 'Not defined'";
                sqlCommand.ExecuteScalar();
            }
            catch(SqlException)
            {
                connectionIsOk = false;
            }
            return connectionIsOk;
        }

        /// <summary>
        /// Test to connect to the database database 
        /// </summary>
        /// <param name="database">The name of the database</param>
        /// <returns>True if it is success otherwise false</returns>
        public bool TestConnection(string database)
        {
            if (database == null)
            {
                database = "master";
            }
            bool isOk;
            SqlConnection conn = null;
            try
            {
                conn = GrabConnection(database);
                //get version of sql server, not really used now, but maybe for future use
                using (DataSet ds = Execute("SELECT SERVERPROPERTY('productversion')", "master"))
                {

                }
                isOk = true;
            }
            finally
            {
                ReturnConnection(conn);
            }
            return isOk;
        }

        /// <summary>
        /// Checks what role the user have on the sql server, if the user is not SA it will check if the user is DBO on all the databses
        /// </summary>
        /// <param name="databases">The databses to check the permission on</param>
        /// <returns>returns the right, could be SA, DBO,limited or None</returns>
        public Right ConnectType(IList<string> databases)
        {
            //Connect to the database
            if (TestConnection(null))//We could open a connection to master
            {
                //Test if the SQL acount that we want in the connection string is sysadmin or dbOwner in all databses
                if (IsSRMember("master", "sysadmin"))
                {
                    return Right.SA;//Could login with system admin rights
                }
                try
                {//If the user dosen't have permission we won't be able to open a connection to the databse
                    foreach (string database in databases)
                    {
                        if (!IsDBMember(database, "db_owner"))
                        {
                            return Right.Limited;//Could login but not with atleast dbOwner rights on the databases
                        }
                    }
                    return Right.DBOwner; //Could login with dbOwner rights on the databases
                }
                catch (SqlException)
                {
                    return Right.Limited;//Could login but not with atleast dbOwner rights on the databases
                }
            } //End test connection
            return Right.None;
        }


        /// <summary>
        /// Executes a SQL Command and returns a DataSet with results
        /// </summary>
        /// <param name="command">The SQL Command to execute</param>
        /// <param name="database">name of the database to execute the command on</param>
        /// <returns>A DataSet with the returning ResultSet(s)</returns>
        private DataSet Execute(string command, string database)
        {
            DataSet ds = new DataSet();
            ds.Locale = System.Globalization.CultureInfo.InvariantCulture;
            SqlConnection conn = null;
            try
            {
                conn = GrabConnection(database);

                using (SqlCommand cmd = new SqlCommand(command, conn))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        adapter.Fill(ds);
                    }
                }
            }
            finally
            {
                ReturnConnection(conn);

            }
            return ds;
        }

        /// <summary>
        /// Will try to grab a SQL connection to a specific database
        /// </summary>
        /// <param name="database">The Databse to connect to</param>
        /// <returns>The connection to the database</returns>
        private SqlConnection GrabConnection(string database)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            conn.Open();
            conn.ChangeDatabase(database);

            return conn;
        }

        /// <summary>
        /// Will Try to close the a sql connection
        /// </summary>
        /// <param name="connection">The SQL connection to close</param>
        private static void ReturnConnection(SqlConnection connection)
        {
            try
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
            catch (SqlException) { }
        }




        /// <summary>
        /// Updates some database parameters on a databse
        /// </summary>
        /// <param name="aggregationDB">the database name that we will insert into some tables</param>
        /// <param name="analyticDB">The database that the changes will be executed on</param>
        public void UpdateCrossRef(string aggregationDB, string analyticDB)
        {
            SqlConnection conn = null;
            SqlTransaction tran = null;

            try
            {
                conn = GrabConnection(analyticDB);
                tran = conn.BeginTransaction();
                using (SqlCommand cmd = new SqlCommand("mart.sys_crossdatabaseview_target_update", conn))
                {

                    cmd.Transaction = tran;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "mart.sys_crossdatabaseview_target_update";


                    cmd.Parameters.Add(new SqlParameter("@defaultname", "TeleoptiCCCAgg"));
                    cmd.Parameters.Add(new SqlParameter("@customname", aggregationDB));
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();


                    cmd.CommandText = "mart.sys_crossDatabaseView_load";
                    cmd.ExecuteNonQuery();
                    cmd.CommandType = CommandType.Text;

                    tran.Commit();
                }
            }
            catch (SqlException)
            {
                if (tran != null)
                {
                    tran.Rollback();
                }
                throw;
            }
            finally
            {
                ReturnConnection(conn);
            }
        }


        /// <summary>
        /// Gets a list of available databases on the connected server
        /// </summary>
        public IEnumerable<string> Databases
        {
            get
            {
                List<string> dbs = new List<string>();
                using (DataSet ds = Execute("select name from sysdatabases where name not in('master', 'tempdb', 'msdb', 'model')", "master"))
                {
                    dbs.Add("---------------"); //blank row...
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        dbs.Add(row[0].ToString());
                    }
                }
                return dbs;
            }
        }

        /// <summary>
        /// Gets a list of available SQLUsers on the connected server
        /// </summary>
        public IEnumerable<string> DBUsers
        {
            get
            {
                List<string> dbUsers = new List<string>();
                using (DataSet ds = Execute("select name,'SQL_User' as user_type from sys.sql_logins where is_disabled = 0", "master"))
                {
                    dbUsers.Add("---------------"); //blank row...
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (!row[0].ToString().StartsWith("##", StringComparison.Ordinal))
                        {
                            dbUsers.Add(row[0].ToString());
                        }
                    }
                }
                return dbUsers;
            }
        }

        public string ConnectionString
        {
            get { return _connString; }
        }

        public string GetDataFolder()
        {
            var dbFolder = new List<string>();
            using (var ds = Execute("select physical_name from sys.database_files where physical_name like '%master%'", "master")
                )
            {
                dbFolder.AddRange(from DataRow row in ds.Tables[0].Rows
                                  where !row[0].ToString().StartsWith("##", StringComparison.Ordinal)
                                  select row[0].ToString());
            }
            // ReSharper disable PossibleNullReferenceException
            return dbFolder.FirstOrDefault().Replace(@"\master.mdf", "");
            // ReSharper restore PossibleNullReferenceException
        }

    }
}