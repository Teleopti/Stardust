using System.Globalization;
using System;
using System.Data.SqlClient;

namespace AnalysisServicesManager
{
    public class CubeSourceFormat
    {
        private string _pre;
        private const string versionCommand = "select convert(char(20), serverproperty('ProductVersion'))";

        public CubeSourceFormat(string pre)
        {
            _pre = pre;
        }

        public string FindAndReplace(CommandLineArgument argument)
        {

            string sqlConnectionString;

            if (argument.UseIntegratedSecurity)
            {
                sqlConnectionString =
                string.Format(CultureInfo.InvariantCulture,
                              "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;Integrated Security=SSPI;Initial Catalog={1}",
                              argument.SqlServer, argument.SqlDatabase);
            }
            else
            {
                sqlConnectionString =
                    string.Format(CultureInfo.InvariantCulture,
                                  "Application Name=TeleoptiPM;Data Source={0};Persist Security Info=True;User ID={1};Password={2};Initial Catalog={3}",
                                  argument.SqlServer, argument.SqlUser, argument.SqlPassword, argument.SqlDatabase);
            }

            var SqlDatabaseName = argument.AnalysisDatabase;
            int dbVersion = version(sqlConnectionString);
            sqlConnectionString = fixConnString(dbVersion, sqlConnectionString);
            string post = _pre;
            post = post.Replace(@"#(AS_DATABASE)", SqlDatabaseName);
            post = post.Replace(@"#(SQL_DATABASE_NAME)", SqlDatabaseName);
            post = post.Replace(@"#(SQL_CONN_STRING)", sqlConnectionString);

            return post;
        }

        private static string fixConnString(int version, string connString)
        {
            switch (version)
            {
                case 9:
                    {
                        return "Provider=SQLNCLI.1;" + connString;
                    }
                case 10:
                    {
                        return "Provider=SQLNCLI10.1;" + connString;
                    }
				case 11:
					{
					return "Provider=SQLNCLI11.1;" + connString;
					}
                case 12:
                    {
                        return "Provider=SQLNCLI11.1;" + connString;
                    }
            }
            throw new ArgumentOutOfRangeException("Unknown sql version: " + version);
        }

        private static int version(string connString) 
        {
            string res;
            using (var conn = new SqlConnection(connString)) 
            {
                conn.Open();
                using (var cmd = new SqlCommand(versionCommand, conn)) 
                {
                    using (var reader = cmd.ExecuteReader()) 
                    {
                        reader.Read();
                        res = reader.GetString(0).Split(new Char[]{'.'})[0];
                    }
                }
            }

            return Int32.Parse(res, CultureInfo.InvariantCulture);
        }
    }
}