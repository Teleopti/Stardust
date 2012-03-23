using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Sql;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics.Tables;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class TodayDate : IAnalyticsDataSetup, IDateData
	{
		public DataTable Table { get; set; }

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture)
		{
			Table = dim_date.CreateTable();

			var date = DateTime.Now.Date;
			Table.AddDate(
				0,
				date,
				date.Year,
				date.Year * 100 + date.Month,
				date.Month,
				analyticsDataCulture.DateTimeFormat.GetMonthName(date.Month),
				date.Month.GetMonthResourceKey(),
				date.Day,
				(int)date.DayOfWeek,
				analyticsDataCulture.DateTimeFormat.GetDayName(date.DayOfWeek),
				date.DayOfWeek.GetWeekDayResourceKey(),
				DateHelper.WeekNumber(date, analyticsDataCulture),
				(date.Year * 100 + DateHelper.WeekNumber(date, analyticsDataCulture)).ToString(analyticsDataCulture),
				date.Year + "Q" + DateHelper.GetQuarter(date.Month)
				);

			Bulk.Insert(connection, Table);
		}

	}
}