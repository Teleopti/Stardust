using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class FillBridgeQueueWorkloadFromData : IAnalyticsDataSetup
	{
		private readonly IWorkloadData _workloads;
		private readonly IQueueData _queues;
		private readonly IBusinessUnitData _businessUnits;
		private readonly IDatasourceData _datasource;

		public FillBridgeQueueWorkloadFromData(IWorkloadData workloads, IQueueData queues, IBusinessUnitData businessUnits, IDatasourceData datasource)
		{
			_workloads = workloads;
			_queues = queues;
			_businessUnits = businessUnits;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var dim_workload = _workloads.Rows;
			var dim_queue = _queues.Rows;

			var query = from w in dim_workload
			            from q in dim_queue
			            select new
			                   	{
									workload_id = (int) w["workload_id"],
									skill_id = (int) w["skill_id"],
			                   		queue_id = (int) q["queue_id"],
								};

			var table = bridge_queue_workload.CreateTable();

			query.ForEach(
				a => table.AddBridgeQueueWorkload(
					a.queue_id,
					a.workload_id,
					a.skill_id,
					_businessUnits.BusinessUnitId,
					_datasource.RaptorDefaultDatasourceId)
				);

			Bulk.Insert(connection, table);
		}
	}
}