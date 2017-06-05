using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using Teleopti.Ccc.Intraday.TestApplication.Infrastructure;

namespace Teleopti.Ccc.Intraday.TestApplication
{
	 public class QueueDataPersister : IQueueDataPersister
	 {
		  private readonly string _connectionString;

		  public QueueDataPersister(string connectionString)
		  {
				_connectionString = connectionString;
		  }

		  public void Persist(IDictionary<int, IList<QueueInterval>> queueData, string timeZoneCode, DateTime dateToDelete)
		  {
				var table = FactQueue.CreateTable();

				foreach (var data in queueData)
				{
					 foreach (var queueInterval in data.Value)
					 {
						  table.AddFact(
								queueInterval.DateId,
								queueInterval.IntervalId,
								queueInterval.QueueId,
								queueInterval.OfferedCalls,
								queueInterval.AnsweredCalls,
								queueInterval.AnsweredCallsWithinSL,
								queueInterval.AbandonedCalls,
								0,
								0,
								0,
								0,
								queueInterval.TalkTime,
								queueInterval.Acw,
								queueInterval.HandleTime,
								queueInterval.SpeedOfAnswer,
								0,
								0,
								0,
								queueInterval.DatasourceId);
					 }
				}

				deleteQueueStatsForToday(dateToDelete, timeZoneCode);
				bulkInsert(table);
		  }

		  private void deleteQueueStatsForToday(DateTime dateToDelete, string timeZoneCode)
		  {
				var dbCommand = new DatabaseCommand(CommandType.StoredProcedure, "mart.web_intraday_simulator_delete_stats", _connectionString);
				var parameterList = new[]
				{
					 new SqlParameter("today", dateToDelete.Date),
					 new SqlParameter("@time_zone_code", timeZoneCode)
				};
				dbCommand.ExecuteNonQuery(parameterList);
		  }

		  private int bulkInsert(DataTable dataTable)
		  {
				if (dataTable == null)
					 return 0;

				var tableName = "mart.fact_queue";
				new BulkWriter().WriteWithRetries(dataTable, _connectionString, tableName);
				Trace.WriteLine("Rows bulk-inserted into '" + tableName + "' : " + dataTable.Rows.Count);
				return dataTable.Rows.Count;
		  }
	 }
}