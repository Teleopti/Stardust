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
	public class BrasilTimeZone : IAnalyticsDataSetup
	{
		
		public IEnumerable<DataRow> Rows { get; set; }

		public int TimeZoneId { get; set; }
		

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_time_zone.CreateTable())
			{
				var timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");
				table.AddTimeZone(
					TimeZoneId,
					timeZone.Id,
					timeZone.DisplayName,
					false,
					(int)timeZone.BaseUtcOffset.TotalMinutes,
					0,
					-1
					);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}