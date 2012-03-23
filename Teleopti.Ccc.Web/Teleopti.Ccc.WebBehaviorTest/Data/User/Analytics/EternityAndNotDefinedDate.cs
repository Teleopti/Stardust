using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class EternityAndNotDefinedDate : IAnalyticsDataSetup, IDateData
	{
		public DataTable Table { get; private set; }

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_date.CreateTable();

			Table.AddRow(-2, new DateTime(2059, 12, 31), 0, 0, 0, "Eternity", null, 0, 0, "Eternity", null, 0, "000000", "ET");
			Table.AddRow(-1, new DateTime(1900, 1, 1), 0, 0, 0, "Not Defined", null, 0, 0, "Not Defined", null, 0, "000000", "ND");
			
			Bulk.Insert(connection, Table);
		}

	}
}