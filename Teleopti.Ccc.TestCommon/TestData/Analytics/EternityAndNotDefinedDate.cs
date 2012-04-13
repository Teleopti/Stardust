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
	public class EternityAndNotDefinedDate : IAnalyticsDataSetup
	{
		public IEnumerable<DataRow> Rows { get; private set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_date.CreateTable())
			{
				table.AddDate(-2, new DateTime(2059, 12, 31), 0, 0, 0, "Eternity", null, 0, 0, "Eternity", null, 0, "000000", "ET");
				table.AddDate(-1, new DateTime(1900, 1, 1), 0, 0, 0, "Not Defined", null, 0, 0, "Not Defined", null, 0, "000000", "ND");

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}