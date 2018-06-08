using System.Data;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics.Tables
{
	public static class queues
	{
		public static DataTable CreateTable()
		{
			var table = new DataTable("dbo.queues");
			table.Columns.Add("queue", typeof(int));
			table.Columns.Add("orig_desc", typeof(string));
			table.Columns.Add("log_object_id", typeof(int));
			table.Columns.Add("orig_queue_id", typeof(int));
			table.Columns.Add("display_desc", typeof(string));
			return table;
		}

		public static void AddAgentLogg(
			this DataTable dataTable,
			int queue,
			string origDesc,
			int logObjectId,
			int origQueuetId,
			string displayDesc
		)
		{
			var row = dataTable.NewRow();
			row["queue"] = queue;
			row["orig_desc"] = origDesc;
			row["log_object_id"] = logObjectId;
			row["orig_queue_id"] = origQueuetId;
			row["display_desc"] = displayDesc;
			dataTable.Rows.Add(row);
		}
	}
}