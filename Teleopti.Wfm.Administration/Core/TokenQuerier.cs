using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;

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
				
                using (var sqlCommand = new SqlCommand(string.Format("SELECT COUNT(*) FROM Tenant.AdminUser where AccessToken ='{0}'",token), sqlConn))
				{
					cnt = (int)sqlCommand.ExecuteScalar();
				}
				sqlConn.Close();
			}
			return cnt > 0;
		}
	}
}