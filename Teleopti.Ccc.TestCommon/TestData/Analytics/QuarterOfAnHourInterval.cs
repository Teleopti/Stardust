using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Sql;
using Teleopti.Ccc.TestCommon.TestData.Analytics.Tables;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.TestCommon.TestData.Analytics
{
	public class QuarterOfAnHourInterval : IIntervalData
	{
		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var table = dim_interval.CreateTable();
			var interval = TimeSpan.FromMinutes(15);
			var start = new DateTime(1900, 1, 1);
			var end = start.AddHours(24);

			start.TimeRange(end.AddMilliseconds(-1), interval)
				.Select((time, index) => new { time, index })
				.ForEach(a =>
				         	{
				         		var time = a.time;
				         		var index = a.index;
				         		var formattedTime = time.ToString(analyticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var id = index;
				         		var intervalStart = time;
				         		var intervalEnd = time.Add(interval);
				         		var intervalName = formattedTime + "-" + intervalEnd.ToString(analyticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var halfHourName = formattedTime + "-" + time.Add(TimeSpan.FromMinutes(30)).ToString(analyticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var hourName = formattedTime + "-" + time.Add(TimeSpan.FromHours(1)).ToString(analyticsDataCulture.DateTimeFormat.ShortTimePattern);

				         		table.AddInterval(
				         			id,
				         			intervalName,
				         			halfHourName,
				         			hourName,
				         			intervalStart,
				         			intervalEnd,
				         			1
				         			);

				         	}
				);

			Bulk.Insert(connection, table);

			Rows = table.AsEnumerable();
		}

	}
}