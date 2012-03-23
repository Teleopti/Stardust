using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class FillBridgeQueueWorkloadFromData : IAnalyticsDataSetup
	{
		private readonly IWorkloadData _workloads;
		private readonly IQueueData _queues;
		private readonly IBusinessUnitData _businessUnits;

		public FillBridgeQueueWorkloadFromData(IWorkloadData workloads, IQueueData queues, IBusinessUnitData businessUnits)
		{
			_workloads = workloads;
			_queues = queues;
			_businessUnits = businessUnits;
		}

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var dim_workload = _workloads.Table.AsEnumerable();
			var dim_queue = _queues.Table.AsEnumerable();

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
				a => table.AddRow(
					a.queue_id,
					a.workload_id,
					a.skill_id,
					_businessUnits.BusinessUnitId,
					sys_datasource.RaptorDefaultDatasourceId)
				);
		}
	}
}