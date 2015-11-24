using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantLogger: IUpgradeLog
	{
		private readonly string _tenant;
		private readonly int _tenantId;

		public TenantLogger(string tenant, int tenantId)
		{
			_tenant = tenant;
			_tenantId = tenantId;
		}

		public void Write(string message)
		{
			Write(message, "DEBUG");
		}

		public void Write(string message, string level)
		{
			var sql = "insert into Tenant.UpgradeLog (Tenant, Time, Level, Message, TenantId) values(@tenant, @time, @level, @message, @tenantid)";

			using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["Tenancy"].ConnectionString))
			{
				sqlConnection.Open();
				using (var sqlCommand = new SqlCommand(sql, sqlConnection))
				{
					sqlCommand.Parameters.AddWithValue("@tenant", _tenant);
					sqlCommand.Parameters.AddWithValue("@time", DateTime.Now);
					sqlCommand.Parameters.AddWithValue("@level", level);
					sqlCommand.Parameters.AddWithValue("@message", message);
					sqlCommand.Parameters.AddWithValue("@tenantid", _tenantId);
					sqlCommand.ExecuteNonQuery();
				}
			}
		}

		public void Dispose()
		{
			
		}
	}
}