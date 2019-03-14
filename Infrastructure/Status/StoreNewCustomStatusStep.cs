using System;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;

namespace Teleopti.Ccc.Infrastructure.Status
{
	public class StoreNewCustomStatusStep
	{
		private readonly Lazy<string> _connectionString;
		
		private const string sql = @"
insert into status.CustomStatusStep (name, description, secondslimit) values
(@name, @description, @secondslimit)";
		
		public StoreNewCustomStatusStep(IConfigReader config)
		{
			_connectionString = new Lazy<string>(() => config.ConnectionString("Status"));
		}
		
		public void Execute(string name, string description, TimeSpan limit)
		{
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				using(var cmd = new SqlCommand(sql, conn))
				{
					cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = name;
					cmd.Parameters.Add("@description", SqlDbType.NVarChar).Value = description;
					cmd.Parameters.Add("@secondsLimit", SqlDbType.Int).Value = limit.TotalSeconds;
					conn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}