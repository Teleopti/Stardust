using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class NotDefinedWorkload : IWorkloadData
	{
		private const string notDefined = "Not Defined";
		private const int workloadId = -1;
		private const bool isDeleted = false;

		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_workload.CreateTable())
			{
				table.AddWorkload(workloadId, Guid.Empty, notDefined, -1, Guid.Empty, notDefined, -1,
					Guid.Empty, notDefined, 1, 1, -1, -1, 0, 1, 1, -1, -1, isDeleted);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}