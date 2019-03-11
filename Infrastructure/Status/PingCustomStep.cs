using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Status
{
	public class PingCustomStep
	{
		private readonly Lazy<string> _connectionString;

		private const string sql = "update status.CustomStatusStep set lastping=getdate() where name=@name";

		public PingCustomStep(IConfigReader config)
		{
			_connectionString = new Lazy<string>(() => config.ConnectionString("Status"));
		}
		
		public bool Execute(string stepName)
		{
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				using(var cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = stepName;
					conn.Open();
					return cmd.ExecuteNonQuery() > 0;
				}
			}
		}
	}
}