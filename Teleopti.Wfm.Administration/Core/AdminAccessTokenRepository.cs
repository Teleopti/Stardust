using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI.WebControls.WebParts;

namespace Teleopti.Wfm.Administration.Core
{
	public class AdminAccessTokenRepository
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

		public static void CreateNewToken()
		{
			throw new NotImplementedException();
		}

		public static bool CheckAndRenewToken()
		{
			throw new NotImplementedException();
		}
	}
}