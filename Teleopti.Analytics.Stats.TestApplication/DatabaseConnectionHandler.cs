using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Analytics.Stats.TestApplication
{
	public class DatabaseConnectionHandler
	{
		private static SqlConnection martConnection()
		{
			var connectionString = ConfigurationManager.ConnectionStrings["AnalyticsDb"].ToString();
			if (connectionString != "")
				return new SqlConnection(connectionString);
			return null;
		}


		public DimDateInfo GetNumberOfDaysInDimDateTable()
		{
			using (var connection = martConnection())
			using (var command = connection.CreateCommand())
			{
				command.CommandType = CommandType.Text;
				command.CommandText = "mart.sys_dim_date_count";

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					return new DimDateInfo
					{
						NumberOfDays = reader.GetInt32(reader.GetOrdinal("number_of_days")),
						StartDate = reader.GetDateTime(reader.GetOrdinal("start_date")).AddDays(1)
					};
				}
				reader.Close();
			}
			return new DimDateInfo { NumberOfDays = -1 };
		}
	}
}