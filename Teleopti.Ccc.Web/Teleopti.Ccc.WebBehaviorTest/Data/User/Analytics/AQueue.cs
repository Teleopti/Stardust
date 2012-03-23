using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class AQueue : IAnalyticsDataSetup, IQueueData
	{
		private readonly IDatasourceData _datasource;

		public int QueueId = 0;

		public DataTable Table { get; private set; }

		public AQueue(IDatasourceData datasource) {
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			Table = dim_workload.CreateTable();

			Table.AddQueue(QueueId, 2, "10001", "Queue 1", "Queue 1", "Log Object", _datasource.RaptorDefaultDatasourceId);
		}

	}
}