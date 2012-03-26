using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class EternityAndNotDefinedDate : IAnalyticsDataSetup
	{
		public IEnumerable<DataRow> Rows { get; private set; }

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
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