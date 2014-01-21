using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class fact_agent_queue
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.fact_agent_queue");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("queue_id", typeof(int));
			table.Columns.Add("acd_login_id", typeof(int));
			table.Columns.Add("local_date_id", typeof(int));
			table.Columns.Add("local_interval_id", typeof(int));
			table.Columns.Add("talk_time_s", typeof(decimal));
			table.Columns.Add("after_call_work_time_s", typeof(decimal));
			table.Columns.Add("answered_calls", typeof(int));
			table.Columns.Add("transfered_calls", typeof(int));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddFactAgentQueue(
			this DataTable dataTable,
			int dateId,
			int intervalId,
			int queueId,
			int acdLoginId,
			int localDateId,
			decimal talkTimeS,
			decimal afterCallWorkTimeS,
			decimal answeredCallsWithinSl,
			int answeredCalls,
			int transferedCalls,
			int datasourceId)
		{
			var row = dataTable.NewRow();
			row["date_id"] = dateId;
			row["interval_id"] = intervalId;
			row["queue_id"] = queueId;
			row["acd_login_id"] = acdLoginId;
			row["local_date_id"] = localDateId;
			row["local_interval_id"] = 95;
			row["talk_time_s"] = talkTimeS;
			row["after_call_work_time_s"] = afterCallWorkTimeS;
			row["answered_calls"] = answeredCalls;
			row["transfered_calls"] = transferedCalls;
			row["datasource_id"] = 999;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}