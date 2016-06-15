using System;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class SpecificTimeZone : IAnalyticsDataSetup
	{
		private readonly TimeZoneInfo _timeZone;
		private readonly int _id;

		public SpecificTimeZone(TimeZoneInfo timeZone, int id=123)
		{
			_timeZone = timeZone;
			_id = id;
		}

		public SpecificTimeZone(string timezone) : this(TimeZoneInfo.FindSystemTimeZoneById(timezone))
		{
			
		}

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			using (var table = dim_time_zone.CreateTable())
			{
				table.AddTimeZone(
					_id,
					_timeZone.Id,
					_timeZone.DisplayName,
					false,
					(int)_timeZone.BaseUtcOffset.TotalMinutes,
					0,
					-1
					);
				Bulk.Insert(connection, table);
			}
		}
	}
}