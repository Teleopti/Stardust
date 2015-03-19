using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Teleopti.Ccc.Web.Areas.Mart.Models;

namespace Teleopti.Ccc.Web.Areas.Mart.Core
{
	public class QueueStatRepository : IQueueStatRepository
	{
		private readonly IDatabaseConnectionHandler _databaseConnectionHandler;
		private int _latency = 0;

		public QueueStatRepository(IDatabaseConnectionHandler databaseConnectionHandler)
		{
			_databaseConnectionHandler = databaseConnectionHandler;
		}

		public LogObjectSource GetLogObject(int logObjectId, string nhibDataSourceName)
		{
			const string sqlText = @"SELECT  d.[datasource_id],  t.time_zone_code FROM[mart].[sys_datasource] d
															INNER JOIN [mart].[dim_time_zone] t ON d.time_zone_id = t.time_zone_id 
															WHERE d.datasource_id = @datasourceId";

			LogObjectSource logObject = null;
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName, _latency))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = sqlText;
				command.Parameters.AddWithValue("@datasourceId", logObjectId);

				connection.Open();
				var reader = command.ExecuteReader(CommandBehavior.CloseConnection);
				while (reader.Read())
				{
					logObject = new LogObjectSource
					{
						Id = reader.GetInt16(reader.GetOrdinal("datasource_id")),
						TimeZoneCode = reader.GetString(reader.GetOrdinal("time_zone_code"))

					};
				}
				reader.Close();
			}

			return logObject;
		}

		public int GetQueueId(string queueName, string queueId, int logObjectId, string nhibDataSourceName)
		{
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName, _latency))
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
			const string sqlText = @"SELECT [date_id] FROM [mart].[dim_date] WHERE date_date = @date";
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName, _latency))
			{
				var command = connection.CreateCommand();
				command.CommandType = CommandType.Text;
				command.CommandText = sqlText;
				command.Parameters.AddWithValue("@date", dateTime.Date);

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

			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName, _latency))
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

		public void SaveBatch(IList<FactQueueModel> factQueueModels, string nhibDataSourceName)
		{
			using (var connection = _databaseConnectionHandler.MartConnection(nhibDataSourceName, _latency))
			{
				var dataRows = getTable(factQueueModels);
				var adapter = new SqlDataAdapter();
				adapter.InsertCommand = new SqlCommand("mart.fact_queue_save", connection)
				{
					CommandType = CommandType.StoredProcedure,
					UpdatedRowSource = UpdateRowSource.None
				};
				// Set the Parameter with appropriate Source Column Name

				adapter.InsertCommand.Parameters.Add("@date_id", SqlDbType.Int, 4, dataRows.Columns[0].ColumnName);
				adapter.InsertCommand.Parameters.Add("@interval_id", SqlDbType.SmallInt, 2, dataRows.Columns[1].ColumnName);
				adapter.InsertCommand.Parameters.Add("@queue_id", SqlDbType.Int, 4, dataRows.Columns[2].ColumnName);
				adapter.InsertCommand.Parameters.Add("@offered_calls", SqlDbType.Decimal, 19, dataRows.Columns[3].ColumnName);
				adapter.InsertCommand.Parameters.Add("@answered_calls", SqlDbType.Decimal, 19, dataRows.Columns[4].ColumnName);
				adapter.InsertCommand.Parameters.Add("@answered_calls_within_SL", SqlDbType.Decimal, 19, dataRows.Columns[5].ColumnName);
				adapter.InsertCommand.Parameters.Add("@abandoned_calls", SqlDbType.Decimal, 19, dataRows.Columns[6].ColumnName);
				adapter.InsertCommand.Parameters.Add("@abandoned_calls_within_SL", SqlDbType.Decimal, 19, dataRows.Columns[7].ColumnName);
				adapter.InsertCommand.Parameters.Add("@abandoned_short_calls", SqlDbType.Decimal, 19, dataRows.Columns[8].ColumnName);
				adapter.InsertCommand.Parameters.Add("@overflow_out_calls", SqlDbType.Decimal, 19, dataRows.Columns[9].ColumnName);
				adapter.InsertCommand.Parameters.Add("@overflow_in_calls", SqlDbType.Decimal, 19, dataRows.Columns[10].ColumnName);
				adapter.InsertCommand.Parameters.Add("@talk_time_s", SqlDbType.Decimal, 19, dataRows.Columns[11].ColumnName);
				adapter.InsertCommand.Parameters.Add("@after_call_work_s", SqlDbType.Decimal, 19, dataRows.Columns[12].ColumnName);
				adapter.InsertCommand.Parameters.Add("@handle_time_s", SqlDbType.Decimal, 19, dataRows.Columns[13].ColumnName);
				adapter.InsertCommand.Parameters.Add("@speed_of_answer_s", SqlDbType.Decimal, 19, dataRows.Columns[14].ColumnName);
				adapter.InsertCommand.Parameters.Add("@time_to_abandon_s", SqlDbType.Decimal, 19, dataRows.Columns[15].ColumnName);
				adapter.InsertCommand.Parameters.Add("@longest_delay_in_queue_answered_s", SqlDbType.Decimal, 19, dataRows.Columns[16].ColumnName);
				adapter.InsertCommand.Parameters.Add("@longest_delay_in_queue_abandoned_s", SqlDbType.Decimal, 19, dataRows.Columns[17].ColumnName);
				adapter.InsertCommand.Parameters.Add("@datasource_id", SqlDbType.SmallInt, 2, dataRows.Columns[18].ColumnName);

				// Specify the number of records to be Inserted/Updated in one go. Default is 1.
				adapter.UpdateBatchSize = 20;

				connection.Open();
				int recordsInserted = adapter.Update(dataRows);
				
			}
		}

		private static DataTable getTable(IEnumerable<FactQueueModel> factQueueModels)
		{
			var table = new DataTable("mart.fact_queue") { Locale = Thread.CurrentThread.CurrentCulture };
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("queue_id", typeof(int));
			table.Columns.Add("offered_calls", typeof(double));
			table.Columns.Add("answered_calls", typeof(double));
			table.Columns.Add("answered_calls_within_SL", typeof(double));
			table.Columns.Add("abandoned_calls", typeof(double));
			table.Columns.Add("abandoned_calls_within_SL", typeof(double));
			table.Columns.Add("abandoned_short_calls", typeof(double));
			table.Columns.Add("overflow_out_calls", typeof(double));
			table.Columns.Add("overflow_in_calls", typeof(double));
			table.Columns.Add("talk_time_s", typeof(double));
			table.Columns.Add("after_call_work_s", typeof(double));
			table.Columns.Add("handle_time_s", typeof(double));
			table.Columns.Add("speed_of_answer_s", typeof(double));
			table.Columns.Add("time_to_abandon_s", typeof(double));
			table.Columns.Add("longest_delay_in_queue_answered_s", typeof(double));
			table.Columns.Add("longest_delay_in_queue_abandoned_s", typeof(double));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));

			foreach (var model in factQueueModels)
			{
				DataRow row = table.NewRow();
				row["date_id"] = model.DateId;
				row["interval_id"] = model.IntervalId;
				row["queue_id"] = model.QueueId;
				row["offered_calls"] = model.OfferedCalls;
				row["answered_calls"] = model.AnsweredCalls;
				row["answered_calls_within_SL"] = model.AnsweredCallsWithinServiceLevel;
				row["abandoned_calls"] = model.AbandonedCalls;
				row["abandoned_calls_within_SL"] = model.AbandonedCallsWithinServiceLevel;
				row["abandoned_short_calls"] = model.AbandonedShortCalls;
				row["overflow_out_calls"] = model.OverflowOutCalls;
				row["overflow_in_calls"] = model.OverflowInCalls;
				row["talk_time_s"] = model.TalkTime;
				row["after_call_work_s"] = model.AfterCallWork;
				row["handle_time_s"] = model.HandleTime;
				row["speed_of_answer_s"] = model.SpeedOfAnswer;
				row["time_to_abandon_s"] = model.TimeToAbandon;
				row["longest_delay_in_queue_answered_s"] = model.LongestDelayInQueueAnswered;
				row["longest_delay_in_queue_abandoned_s"] = model.LongestDelayInQueueAbandoned;
				row["datasource_id"] = model.LogObjectId;

				table.Rows.Add(row);
			}

			return table;
		}

		public void SetLatency(int latency)
		{
			_latency = latency;
		}
	}
}