using System;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public class QueueStatRepository :IQueueStatRepository
	{
		private readonly IDatabaseConnectionHandler _databaseConnectionHandler;

		public QueueStatRepository(IDatabaseConnectionHandler databaseConnectionHandler)
		{
			_databaseConnectionHandler = databaseConnectionHandler;
		}

		public LogObject GetLogObject(string logobjectName, string nhibDataSourceName)
		{
			const string sqlText = @"SELECT  d.[datasource_id],  t.time_zone_code FROM[mart].[sys_datasource] d
  INNER JOIN [mart].[dim_time_zone] t ON d.time_zone_id = t.time_zone_id 
  WHERE [datasource_name] = '{0}'";

			LogObject logObject = null;
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = string.Format(sqlText, logobjectName);

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					logObject  = new LogObject
					{
						Id =  reader.GetInt16(reader.GetOrdinal("datasource_id")),
						TimeZoneCode = reader.GetString(reader.GetOrdinal("time_zone_code"))
					
					};
				}
				reader.Close();
			}

			return logObject;
		}

		public int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName)
		{
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.StoredProcedure;
				command.CommandText = "mart.ReturnQueueId";
				command.Parameters.Add(new SqlParameter("@queue_original_id", queueId));
				command.Parameters.Add(new SqlParameter("@queue_name", queueName));
				command.Parameters.Add(new SqlParameter("@datasource_id", logObjectId));

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					return reader.GetInt32(reader.GetOrdinal("id"));
				}
				reader.Close();
			}

			return -1;

		}


		public int GetDateId(DateTime dateTime, string nhibDataSourceName)
		{
			const string sqlText = @"SELECT [date_id] FROM [mart].[dim_date] WHERE date_date = '{0}'";
			var dateString = dateTime.ToString("yyyy-MM-dd");
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = string.Format(sqlText, dateString);

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					return reader.GetInt32(reader.GetOrdinal("date_id"));
				}
				reader.Close();
			}

			return -1;
		}

		public int GetIntervalLength(string nhibDataSourceName)
		{
			const string sqlText = @"SELECT [value] FROM .[mart].[sys_configuration] WHERE [key] = 'IntervalLengthMinutes'";
			
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = sqlText;

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					return int.Parse(reader.GetString(reader.GetOrdinal("value")));
				}
				reader.Close();
			}

			return -1;
		}
	}
}