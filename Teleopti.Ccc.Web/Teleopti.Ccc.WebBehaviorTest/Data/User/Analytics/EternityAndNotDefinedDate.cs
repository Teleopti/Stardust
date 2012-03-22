using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class EternityAndNotDefinedDate : IStatisticsDataSetup
	{
		public DataTable Table;

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_date.CreateTable();

			Table.AddRow(-2, new DateTime(2059, 12, 31), 0, 0, 0, "Eternity", null, 0, 0, "Eternity", null, 0, "000000", "ET", DateTime.Now);
			Table.AddRow(-1, new DateTime(1900, 1, 1), 0, 0, 0, "Not Defined", null, 0, 0, "Not Defined", null, 0, "000000", "ND", DateTime.Now);
			
			Bulk.Insert(connection, Table);
		}
	}
}