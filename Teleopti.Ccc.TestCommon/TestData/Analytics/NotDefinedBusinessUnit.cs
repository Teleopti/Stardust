using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class NotDefinedBusinessUnit : IAnalyticsDataSetup, IBusinessUnitData
	{
		public IEnumerable<DataRow> Rows { get; set; }
		public int BusinessUnitId => -1;

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_business_unit.CreateTable())
			{
				table.AddBusinessUnit(BusinessUnitId, Guid.Empty, "Not Defined", -1);
				Bulk.Insert(connection, table);
				Rows = table.AsEnumerable();
			}
		}
	}
}