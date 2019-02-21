using System;
using System.Configuration;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Administration.Core
{
	public class TenantLogger: IUpgradeLog
	{
		private readonly string _tenant;
		private readonly int _tenantId;
		private readonly IConfigReader _config;

		public TenantLogger(string tenant, int tenantId, IConfigReader config)
		{
			_tenant = tenant;
			_tenantId = tenantId;
			_config = config;
		}

		public void Write(string message)
		{
			Write(message, "DEBUG");
		}

		public void Write(string message, string level)
		{
			var sql = "insert into Tenant.UpgradeLog (Tenant, Time, Level, Message, TenantId) values(@tenant, @time, @level, @message, @tenantid)";

			using (var sqlConnection = new SqlConnection(_config.ConnectionString("Tenancy")))
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