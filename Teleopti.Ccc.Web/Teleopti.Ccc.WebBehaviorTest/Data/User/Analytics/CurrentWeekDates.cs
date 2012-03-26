using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class CurrentWeekDates : IAnalyticsDataSetup, IDateData
	{
		public IEnumerable<DataRow> Rows { get; set; }

		public void Apply(SqlConnection connection, CultureInfo userCulture, CultureInfo analyticsDataCulture)
		{
			var table = dim_date.CreateTable();

			var startDate = DateHelper.GetFirstDateInWeek(DateTime.Now.Date, userCulture);
			var dates = startDate.DateRange(7);

			var id = 0;
			dates.ForEach(date =>
			              	{
								table.AddDate(id, date, analyticsDataCulture);
			              		id++;
			              	});

			Bulk.Insert(connection, table);

			Rows = table.AsEnumerable();
		}
	}

	public static class DateTimeExtensions
	{
		public static IEnumerable<DateTime> DateRange(this DateTime instance, int days)
		{
			return from i in Enumerable.Range(0, days) select instance.AddDays(i);
		}

	}

}