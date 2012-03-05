using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Analytics.ReportTexts;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class DefaultStatisticsTimeZoneData : IStatisticsDataSetup
	{
		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture) {
			PersistDimIntervallData(connection, statisticsDataCulture);
			PersistDimDatelData(connection, statisticsDataCulture);
			PersistDimTimeZoneData(connection, statisticsDataCulture);
		}

		private void PersistDimTimeZoneData(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var table = CreateDimTimeZoneTable();

			var utc = new object[]
			          	{
			          		0, 
			          		"UTC", 
			          		"UTC", 
							0,
							0,
							0,
							-1,
							DateTime.Now,
							DateTime.Now
			          	};

			var cetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var cet = new object[]
			          	{
			          		1,
			          		cetTimeZone,
			          		cetTimeZone.DisplayName,
			          		1,
			          		cetTimeZone.BaseUtcOffset.TotalMinutes,
			          		0,
			          		-1,
			          		DateTime.Now,
			          		DateTime.Now
			          	};
			table.Rows.Add(utc);
			table.Rows.Add(cet);

			BulkInsert(connection, table);
		}

		private DataTable CreateDimTimeZoneTable()
		{
			var table = new DataTable("mart.dim_time_zone");
			table.Columns.Add("time_zone_id");
			table.Columns.Add("time_zone_code");
			table.Columns.Add("time_zone_name");
			table.Columns.Add("default_zone");
			table.Columns.Add("utc_conversion");
			table.Columns.Add("utc_conversion_dst");
			table.Columns.Add("datasource_id");
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			//table.Columns.Add("to_be_deleted");
			//table.Columns.Add("only_one_default_zone");
			return table;
		}

		private void PersistDimDatelData(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var table = CreateDimDateTable();
			var eternityRow = new object[] {-2, new DateTime(2059, 12, 31), 0, 0, 0, "Eternity", null, 0, 0, "Eternity", null, 0, "000000", "ET", DateTime.Now};
			var notDefinedRow = new object[] {-1, new DateTime(1900, 1, 1), 0, 0, 0, "Not Defined", null, 0, 0, "Not Defined", null, 0, "000000", "ND", DateTime.Now};
				
			var date = DateTime.Now.Date;
			var row = new object[]
			          	{
			          		0, 
			          		date, 
			          		date.Year, 
			          		date.Year*100 + date.Month, 
			          		date.Month, 
			          		statisticsDataCulture.DateTimeFormat.GetMonthName(date.Month),
			          		date.Month.GetMonthResourceKey(),
			          		date.Day,
			          		(int) date.DayOfWeek,
			          		statisticsDataCulture.DateTimeFormat.GetDayName(date.DayOfWeek),
			          		date.DayOfWeek.GetWeekDayResourceKey(),
			          		DateHelper.WeekNumber(date, statisticsDataCulture),
			          		(date.Year*100 + DateHelper.WeekNumber(date, statisticsDataCulture)).ToString(statisticsDataCulture),
			          		date.Year + "Q" + DateHelper.GetQuarter(date.Month),
			          		DateTime.Now
			          	};

			table.Rows.Add(eternityRow);
			table.Rows.Add(notDefinedRow);
			table.Rows.Add(row);

			BulkInsert(connection, table);
		}

		private void PersistDimIntervallData(SqlConnection connection, CultureInfo statisticsDataCulture)
		{
			var table = CreateDimIntervalTable();
			var interval = TimeSpan.FromMinutes(15);
			var start = new DateTime(1900, 1, 1);
			var end = start.AddHours(24);

			var rows = start.TimeRange(end.AddMilliseconds(-1), interval)
				.Select((time, index) =>
				        	{
				        		var formattedTime = time.ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				        		var id = index;
				        		var intervalStart = time;
				        		var intervalEnd = time.Add(interval);
				        		var intervalName = formattedTime + "-" + intervalEnd.ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				        		var halfHourName = formattedTime + "-" + time.Add(TimeSpan.FromMinutes(30)).ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				        		var hourName = formattedTime + "-" + time.Add(TimeSpan.FromHours(1)).ToString(statisticsDataCulture.DateTimeFormat.ShortTimePattern);
				        		return new object[]
				        		       	{
				        		       		id,
				        		       		intervalName,
				        		       		halfHourName,
				        		       		hourName,
				        		       		intervalStart,
				        		       		intervalEnd,
				        		       		1,
				        		       		DateTime.Now,
				        		       		DateTime.Now
				        		       	};
				        	})
				;
			rows.ForEach(r => table.Rows.Add((object[]) r));

			BulkInsert(connection, table);
		}

		private DataTable CreateDimIntervalTable()
		{
			var table = new DataTable("mart.dim_interval");
			table.Columns.Add("interval_id");
			table.Columns.Add("interval_name");
			table.Columns.Add("halfhour_name");
			table.Columns.Add("hour_name");
			table.Columns.Add("interval_start");
			table.Columns.Add("interval_end");
			table.Columns.Add("datasource_id");
			table.Columns.Add("insert_date");
			table.Columns.Add("update_date");
			return table;
		}

		private DataTable CreateDimDateTable()
		{
			var table = new DataTable("mart.dim_date");
			table.Columns.Add("date_id");
			table.Columns.Add("date_date");
			table.Columns.Add("year");
			table.Columns.Add("year_month");
			table.Columns.Add("month");
			table.Columns.Add("month_name");
			table.Columns.Add("month_resource_key");
			table.Columns.Add("day_in_month");
			table.Columns.Add("weekday_number");
			table.Columns.Add("weekday_name");
			table.Columns.Add("weekday_resource_key");
			table.Columns.Add("week_number");
			table.Columns.Add("year_week");
			table.Columns.Add("quarter");
			table.Columns.Add("insert_date");
			return table;
		}

		private static void BulkInsert(SqlConnection connection, DataTable table)
		{
			using (var bulk = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, null))
			{
				bulk.DestinationTableName = table.TableName;
				bulk.WriteToServer(table);
			}
		}


	}
}