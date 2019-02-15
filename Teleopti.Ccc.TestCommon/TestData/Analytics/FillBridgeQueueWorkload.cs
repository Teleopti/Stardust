using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class FillBridgeQueueWorkload : IWorkloadData
	{
		private readonly ISkillData _skills;
		private readonly IBusinessUnitData _businessUnits;
		private readonly IDatasourceData _datasource;

		public int QueueId { get; set; }
		public int WorkloadId { get; set; }

		public IEnumerable<DataRow> Rows { get; set; }

		public FillBridgeQueueWorkload(ISkillData skills, IBusinessUnitData businessUnits, IDatasourceData datasource)
		{
			_skills = skills;
			_businessUnits = businessUnits;
			_datasource = datasource;
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = bridge_queue_workload.CreateTable())
			{
				table.AddBridgeQueueWorkload(QueueId, WorkloadId, _skills.FirstSkillId, _businessUnits.BusinessUnitId, _datasource.RaptorDefaultDatasourceId);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}