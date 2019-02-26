using System;
using System.Data.SqlClient;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Domain.ETL;

namespace Teleopti.Ccc.Infrastructure.ETL
{
	public class TimeSinceLastEtlPing : ITimeSinceLastEtlPing, IMarkEtlPing
	{
		private readonly Lazy<string> _connectionString;

		public TimeSinceLastEtlPing(IConfigReader config)
		{
			_connectionString = new Lazy<string>(() => config.AppConfig("DatamartConnectionString"));
		}
		
		public TimeSpan Fetch()
		{
			const string sql = "select datediff(second, date, getdate()) from mart.LastEtlPing";
			
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				conn.Open();
				using(var cmd = new SqlCommand(sql, conn))
				{
					return TimeSpan.FromSeconds((int)cmd.ExecuteScalar());
				}
			}
		}

		public void Store()
		{
			const string sql = "update mart.LastEtlPing set date = getdate()";
			
			using (var conn = new SqlConnection(_connectionString.Value))
			{
				conn.Open();
				var cmd = new SqlCommand(sql, conn);
				cmd.ExecuteNonQuery();
			}
		}
	}
}