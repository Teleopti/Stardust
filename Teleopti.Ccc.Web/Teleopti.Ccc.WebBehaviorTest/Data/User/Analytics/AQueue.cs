using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class AQueue : IAnalyticsDataSetup, IQueueData
	{
		public int QueueId = 0;

		public DataTable Table { get; private set; }

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_workload.CreateTable();

			Table.AddRow(QueueId,2, "10001", "Queue 1", "Queue 1", "Log Object", sys_datasource.RaptorDefaultDatasourceId);
		}

	}
}