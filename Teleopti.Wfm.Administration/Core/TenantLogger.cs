using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.DBManager.Library;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantLogger: IUpgradeLog
	{
		private readonly string _tenant;

		public TenantLogger(string tenant)
		{
			_tenant = tenant;
		}

		public void Write(string message)
		{
			Write(message, "DEBUG");
		}

		public void Write(string message, string level)
		{
			var sql = "insert into Tenant.UpgradeLog (Tenant, Time, Level, Message) values(@tenant, @time, @level, @message)";

			using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@tenant", _tenant);
					sqlCommand.Parameters.AddWithValue("@time", DateTime.Now);
					sqlCommand.Parameters.AddWithValue("@level", level);
					sqlCommand.Parameters.AddWithValue("@message", message);
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public void Dispose()
		{
			
		}
	}
}