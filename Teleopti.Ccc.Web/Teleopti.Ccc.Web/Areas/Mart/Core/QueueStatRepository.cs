using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Teleopti.Ccc.Web.Areas.Mart.Models;

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

		public void Save(IList<FactQueueModel> factQueueModel, string nhibDataSourceName)
		{
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName))
			{
				connection.Open();
				foreach (var queueModel in factQueueModel)
				{
					using (var command = connection.CreateCommand())
					{
						command.CommandType = CommandType.StoredProcedure;
						command.CommandText = "mart.fact_queue_save";
						command.Parameters.Add(new SqlParameter("@datasource_id", queueModel.LogObjectId));
						command.Parameters.Add(new SqlParameter("@date_id", queueModel.DateId));
						command.Parameters.Add(new SqlParameter("@interval_id", queueModel.IntervalId));
						command.Parameters.Add(new SqlParameter("@queue_id", queueModel.QueueId));
						command.Parameters.Add(new SqlParameter("@offered_calls", queueModel.OfferedCalls));
						command.Parameters.Add(new SqlParameter("@answered_calls", queueModel.AnsweredCalls));
						command.Parameters.Add(new SqlParameter("@answered_calls_within_SL", queueModel.AnsweredCallsWithinServiceLevel));
						command.Parameters.Add(new SqlParameter("@abandoned_calls", queueModel.AbandonedCalls));
						command.Parameters.Add(new SqlParameter("@abandoned_calls_within_SL", queueModel.AbandonedCallsWithinServiceLevel));
						command.Parameters.Add(new SqlParameter("@abandoned_short_calls", queueModel.AbandonedShortCalls));
						command.Parameters.Add(new SqlParameter("@overflow_out_calls", queueModel.OverflowOutCalls));
						command.Parameters.Add(new SqlParameter("@overflow_in_calls", queueModel.OverflowInCalls));
						command.Parameters.Add(new SqlParameter("@talk_time_s", queueModel.TalkTime));
						command.Parameters.Add(new SqlParameter("@after_call_work_s", queueModel.AfterCallWork));
						command.Parameters.Add(new SqlParameter("@handle_time_s", queueModel.HandleTime));
						command.Parameters.Add(new SqlParameter("@speed_of_answer_s", queueModel.SpeedOfAnswer));
						command.Parameters.Add(new SqlParameter("@time_to_abandon_s", queueModel.TimeToAbandon));
						command.Parameters.Add(new SqlParameter("@longest_delay_in_queue_answered_s", queueModel.LongestDelayInQueueAnswered));
						command.Parameters.Add(new SqlParameter("@longest_delay_in_queue_abandoned_s", queueModel.LongestDelayInQueueAbandoned));

						command.ExecuteNonQuery();
					}
				}
			}
		}
	}
}