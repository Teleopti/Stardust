using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class AQueue : IAnalyticsDataSetup, IQueueData
	{
		private readonly IDatasourceData _datasource;

		public int QueueId;

		public IEnumerable<DataRow> Rows { get; set; }

		public AQueue(IDatasourceData datasource) { _datasource = datasource; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			// tbh, not completely sure its ok to dispose datatable and still use its rows
			using (var table = dim_queue.CreateTable())
			{
				table.AddQueue(QueueId, 2, "10001", "Queue 1", "Queue 1", "Log Object", _datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}