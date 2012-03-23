using System;
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

		public DataTable Table { get; set; }
		public int UtcTimeZoneId { get; set; }
		public int CetTimeZoneId { get; set; }

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_time_zone.CreateTable();

			var utcTimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");
			Table.AddDimTimeZoneRow(
				UtcTimeZoneId,
				utcTimeZone.Id,
				utcTimeZone.DisplayName,
				false,
				0,
				0,
				-1
				);

			var cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			Table.AddDimTimeZoneRow(
				CetTimeZoneId,
				cetTimeZone.Id,
				cetTimeZone.DisplayName,
				true,
				(int)cetTimeZone.BaseUtcOffset.TotalMinutes,
				0,
				-1
				);

			Bulk.Insert(connection, Table);
		}

	}
}