using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class DefaultSkillset : IAnalyticsDataSetup
	{
		public IEnumerable<DataRow> Rows { get; set; }
		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_skillset.CreateTable())
			{
				table.AddSkillset(-1, "Not Defined", "Not Defined", -1, -1);
				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}
	}
}