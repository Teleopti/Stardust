using System.Globalization;
using System;
using System.Data.SqlClient;

namespace AnalysisServicesManager
{
    public class CubeSourceFormat
    {
	    private readonly ServerConnectionInfo _analysisConnectionInfo;
	    private readonly ServerConnectionInfo _sqlConnectionInfo;
        private const string versionCommand = "select convert(char(20), serverproperty('ProductVersion'))";

		public CubeSourceFormat(ServerConnectionInfo analysisConnectionInfo, ServerConnectionInfo sqlConnectionInfo)
		{
			_analysisConnectionInfo = analysisConnectionInfo;
			_sqlConnectionInfo = sqlConnectionInfo;
		}

	    public string FindAndReplace(string script)
        {
			int dbVersion = version(_sqlConnectionInfo.ConnectionString);
			string sqlConnectionStringWithProvide = AddSQLProvider(dbVersion, _sqlConnectionInfo.ConnectionString);

			var post = script.Replace(@"#(AS_DATABASE_NAME)", _analysisConnectionInfo.DatabaseName);
            post = post.Replace(@"#(SQL_DATABASE_NAME)", _sqlConnectionInfo.DatabaseName);
			post = post.Replace(@"#(SQL_CONN_STRING)", sqlConnectionStringWithProvide);

            return post;
        }

        private static string AddSQLProvider(int version, string connString)
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