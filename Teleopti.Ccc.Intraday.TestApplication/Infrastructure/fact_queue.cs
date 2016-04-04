using System;
using System.Data;

namespace Teleopti.Ccc.Intraday.TestApplication.Infrastructure
{
	public static class FactQueue
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public static DataTable CreateTable()
		{
			var table = new DataTable("mart.fact_queue");
			table.Columns.Add("date_id", typeof(int));
			table.Columns.Add("interval_id", typeof(int));
			table.Columns.Add("queue_id", typeof(int));
			table.Columns.Add("offered_calls", typeof(decimal));
			table.Columns.Add("answered_calls", typeof(decimal));
			table.Columns.Add("answered_calls_within_SL", typeof(decimal));
			table.Columns.Add("abandoned_calls", typeof(decimal));
			table.Columns.Add("abandoned_calls_within_SL", typeof(decimal));
			table.Columns.Add("abandoned_short_calls", typeof(decimal));
			table.Columns.Add("overflow_out_calls", typeof(decimal));
			table.Columns.Add("overflow_in_calls", typeof(decimal));
			table.Columns.Add("talk_time_s", typeof(decimal));
			table.Columns.Add("after_call_work_s", typeof(decimal));
			table.Columns.Add("handle_time_s", typeof(decimal));
			table.Columns.Add("speed_of_answer_s", typeof(decimal));
			table.Columns.Add("time_to_abandon_s", typeof(decimal));
			table.Columns.Add("longest_delay_in_queue_answered_s", typeof(decimal));
			table.Columns.Add("longest_delay_in_queue_abandoned_s", typeof(decimal));
			table.Columns.Add("datasource_id", typeof(int));
			table.Columns.Add("insert_date", typeof(DateTime));
			return table;
		}

		public static void AddFact(
			this DataTable dataTable,
			int date_id,
			int interval_id,
			int queue_id,
			decimal offered_calls,
			decimal answered_calls,
			decimal answered_calls_within_SL,
			decimal abandoned_calls,
			decimal abandoned_calls_within_SL,
			decimal abandoned_short_calls,
			decimal overflow_out_calls,
			decimal overflow_in_calls,
			decimal talk_time_s,
			decimal after_call_work_s,
			decimal handle_time_s,
			decimal speed_of_answer_s,
			decimal time_to_abandon_s,
			decimal longest_delay_in_queue_answered_s,
			decimal longest_delay_in_queue_abandoned_s, 
			int datasource_id
			)
		{
			var row = dataTable.NewRow();
			row["date_id"] = date_id;
			row["interval_id"] = interval_id;
			row["queue_id"] = queue_id;
			row["offered_calls"] = offered_calls;
			row["answered_calls"] = answered_calls;
			row["answered_calls_within_SL"] = answered_calls_within_SL;
			row["abandoned_calls"] = abandoned_calls;
			row["abandoned_calls_within_SL"] = abandoned_calls_within_SL;
			row["abandoned_short_calls"] = abandoned_short_calls;
			row["overflow_out_calls"] = overflow_out_calls;
			row["overflow_in_calls"] = overflow_in_calls;
			row["talk_time_s"] = talk_time_s;
			row["after_call_work_s"] = after_call_work_s;
			row["handle_time_s"] = handle_time_s;
			row["speed_of_answer_s"] = speed_of_answer_s;
			row["time_to_abandon_s"] = time_to_abandon_s;
			row["longest_delay_in_queue_answered_s"] = longest_delay_in_queue_answered_s;
			row["longest_delay_in_queue_abandoned_s"] = longest_delay_in_queue_abandoned_s;
			row["datasource_id"] = datasource_id;
			row["insert_date"] = DateTime.Now;
			dataTable.Rows.Add(row);
		}
	}
}