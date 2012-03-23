using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class QuarterOfAnHourInterval : IAnalyticsDataSetup, IIntervalData
	{
		public DataTable Table { get; private set; }

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_interval.CreateTable();
			var interval = TimeSpan.FromMinutes(15);
			var start = new DateTime(1900, 1, 1);
			var end = start.AddHours(24);

			start.TimeRange(end.AddMilliseconds(-1), interval)
				.Select((time, index) => new { time, index })
				.ForEach(a =>
				         	{
				         		var time = a.time;
				         		var index = a.index;
				         		var formattedTime = time.ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var id = index;
				         		var intervalStart = time;
				         		var intervalEnd = time.Add(interval);
				         		var intervalName = formattedTime + "-" + intervalEnd.ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var halfHourName = formattedTime + "-" + time.Add(TimeSpan.FromMinutes(30)).ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				         		var hourName = formattedTime + "-" + time.Add(TimeSpan.FromHours(1)).ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);

				         		Table.AddRow(
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

			Bulk.Insert(connection, Table);
		}

	}
}