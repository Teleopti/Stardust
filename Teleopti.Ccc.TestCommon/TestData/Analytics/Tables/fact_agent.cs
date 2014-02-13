using System;
using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class fact_agent
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.fact_agent");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("acd_login_id", typeof(int));
			table.Columns.Add("local_date_id", typeof(int));
			table.Columns.Add("local_interval_id", typeof(int));
			table.Columns.Add("ready_time_s", typeof(decimal));
			table.Columns.Add("logged_in_time_s", typeof(decimal));
			table.Columns.Add("not_ready_time_s", typeof(decimal));
			table.Columns.Add("idle_time_s", typeof(decimal));
			table.Columns.Add("direct_outbound_calls", typeof(int));
			table.Columns.Add("direct_outbound_talk_time_s", typeof(decimal));
			table.Columns.Add("direct_incoming_calls", typeof(int));
			table.Columns.Add("direct_incoming_calls_talk_time_s", typeof(decimal));
			table.Columns.Add("admin_time_s", typeof(decimal));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			table.Columns.Add("update_date", typeof(DateTime));
			table.Columns.Add("datasource_update_date", typeof(DateTime));
			return table;
		}

		public static void AddFactAgent(
			this DataTable dataTable,
			int dateId, 
			int intervalId,
			int acdLoginId, 
			int localDateId, 
			int localIntervalId, 
			int readyTimeS, 
			int loggedInTimeS, 
			int notReadyTimeS, 
			int idleTimeS, 
			int directOutboundCalls, 
			int directOutboundTalkTimeS, 
			int directIncomingCalls, 
			int directIncomingCallsTalkTimeS, 
			int adminTimeS)
		{
			var row = dataTable.NewRow();
			row["date_id"] = dateId;
			row["interval_id"] = intervalId;
			row["acd_login_id"] = acdLoginId;
			row["local_date_id"] = localDateId;
			row["local_interval_id"] = localIntervalId;
			row["ready_time_s"] = readyTimeS;
			row["logged_in_time_s"] = loggedInTimeS;
			row["not_ready_time_s"] = notReadyTimeS;
			row["idle_time_s"] = idleTimeS;
			row["direct_outbound_calls"] = directOutboundCalls;
			row["direct_outbound_talk_time_s"] = directOutboundTalkTimeS;
			row["direct_incoming_calls"] = directIncomingCalls;
			row["direct_incoming_calls_talk_time_s"] = directIncomingCallsTalkTimeS;
			row["admin_time_s"] = adminTimeS;
			row["datasource_id"] = 999;
			row["insert_date"] = DateTime.Now;
			row["update_date"] = DateTime.Now;
			row["datasource_update_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}