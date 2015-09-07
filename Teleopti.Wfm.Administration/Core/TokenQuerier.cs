using System.Configuration;
using System.Data.SqlClient;

namespace Teleopti.Wfm.Administration.Core
{
	public class TokenQuerier
	{
		public static bool TokenIsValid(string token)
		{
			int cnt;
			var builder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString);
			using (var sqlConn = new SqlConnection(builder.ConnectionString))
			{
				sqlConn.Open();
				
                using (var sqlCommand = new SqlCommand("SELECT COUNT(*) FROM Tenant.AdminUser where AccessToken = @AccessToken", sqlConn))
                {
	                sqlCommand.Parameters.AddWithValue("@AccessToken", token);
					cnt = (int)sqlCommand.ExecuteScalar();
				}
				sqlConn.Close();
			}
			return cnt > 0;
		}
	}
}