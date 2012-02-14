using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

// Create the login table structure
//C:\Windows\Microsoft.NET\Framework\v2.0.50727>aspnet_regsql -S ANTONOV\SQL2005DEV -d TeleoptiAnalytics_dev -E -A m
namespace Teleopti.Analytics.Portal.Utils
{
    public class LogOnUtilities  : IDisposable
    {
        private readonly SqlConnection _connection = new SqlConnection();

        public LogOnUtilities(string connectionString)
        {
            _connection.ConnectionString = connectionString;
        }

        // TODO Get real data
        public DataTable GetUserInfo(string userName)
        {
            bool windowsAuthentication = false;
            IList<SqlParameter> parameterList = new List<SqlParameter>();
            parameterList.Add(new SqlParameter("@user_name", userName));

            if (userName.Contains("\\")) 
                windowsAuthentication = true;

            parameterList.Add(new SqlParameter("@windows_authentication", windowsAuthentication));

            return GetDataTable("dbo.aspnet_Users_get_user_info", parameterList);
        }

        public bool CheckPassword(string userName, string password)
        {
            IList<SqlParameter> parameterList = new List<SqlParameter>
                                                    {
                                                        new SqlParameter("@ApplicationName", "/"),
                                                        new SqlParameter("@UserName", userName),
                                                        new SqlParameter("@UpdateLastLoginActivityDate", 1),
                                                        new SqlParameter("@CurrentTimeUtc", DateTime.UtcNow)
                                                    };

            DataTable t = GetDataTable("dbo.aspnet_Membership_GetPasswordWithFormat", parameterList);
            if (t==null || t.Rows.Count == 0) return false;

            string p = t.Rows[0].Field<string>(0);
            string p2 = EncryptPassword(password);

            return p.Equals(p2);

        }

        private static string EncryptPassword(string password)
        {
            //TODO! Must check with JN if we can share some code with the other app instead?
            SHA1Managed encryptor = new SHA1Managed();
            const string salt = "adgvabar4g61qt46gv";

            var stringValue = string.Concat(salt, password);
            var hashedBytes = encryptor.ComputeHash(Encoding.UTF8.GetBytes(stringValue));
            return string.Concat("###", BitConverter.ToString(hashedBytes).Replace("-",""), "###");
        }

        private DataTable GetDataTable(string storedProcedureName, IEnumerable<SqlParameter> parameterList)
        {
            DataSet ret = new DataSet();
            ret.Locale = CultureInfo.CurrentCulture;
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
            SqlCommand sqlCommand = new SqlCommand();

            sqlCommand.CommandType = CommandType.StoredProcedure;
            sqlCommand.CommandText = storedProcedureName;
            sqlCommand.Connection = _connection;
            sqlCommand.Parameters.AddRange(parameterList.ToArray());

            
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            sqlDataAdapter.SelectCommand = sqlCommand;
            sqlDataAdapter.Fill(ret, "Data");
            return ret.Tables.Count > 0 ? ret.Tables[0] : null;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool all)
        {
            _connection.Dispose();
        }
        #endregion
    }
}
