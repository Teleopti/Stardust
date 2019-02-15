using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class ATimeZone : ITimeZoneData
	{
		public TimeZoneInfo TimeZoneInfo { get; private set; }

		public IEnumerable<DataRow> Rows { get; set; }

		public int TimeZoneId { get; set; }

		public ATimeZone(string timeZoneCode)
		{
			TimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneCode);
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_time_zone.CreateTable())
			{
				table.AddTimeZone(
					TimeZoneId,
					TimeZoneInfo.Id,
					TimeZoneInfo.DisplayName,
					false,
					(int)TimeZoneInfo.BaseUtcOffset.TotalMinutes,
					0,
					-1
					);

				Bulk.Insert(connection, table);

				Rows = table.AsEnumerable();
			}
		}

	}
}