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

		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			Table = dim_date.CreateTable();

			var date = DateTime.Now.Date;
			Table.AddRow(
				0,
				date,
				date.Year,
				date.Year * 100 + date.Month,
				date.Month,
				statisticsDataCulture.DateTimeFormat.GetMonthName(date.Month),
				date.Month.GetMonthResourceKey(),
				date.Day,
				(int)date.DayOfWeek,
				statisticsDataCulture.DateTimeFormat.GetDayName(date.DayOfWeek),
				date.DayOfWeek.GetWeekDayResourceKey(),
				DateHelper.WeekNumber(date, statisticsDataCulture),
				(date.Year * 100 + DateHelper.WeekNumber(date, statisticsDataCulture)).ToString(statisticsDataCulture),
				date.Year + "Q" + DateHelper.GetQuarter(date.Month)
				);

			Bulk.Insert(connection, Table);
		}

	}
}