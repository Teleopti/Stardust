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
	public class UtcAndCetTimeZones : IAnalyticsDataSetup, ITimeZoneData
	{

		public UtcAndCetTimeZones()
		{
			UtcTimeZoneId = 0;
			CetTimeZoneId = 1;
		}

		public IEnumerable<DataRow> Rows { get; set; }

		public int UtcTimeZoneId { get; set; }
		public int CetTimeZoneId { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_time_zone.CreateTable())
			{
				var utcTimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
				table.AddTimeZone(
					UtcTimeZoneId,
					utcTimeZone.Id,
					utcTimeZone.DisplayName,
					false,
					0,
					0,
					-1
					);

				var cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
				table.AddTimeZone(
					CetTimeZoneId,
					cetTimeZone.Id,
					cetTimeZone.DisplayName,
					true,
					(int)cetTimeZone.BaseUtcOffset.TotalMinutes,
					0,
					-1
					);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}