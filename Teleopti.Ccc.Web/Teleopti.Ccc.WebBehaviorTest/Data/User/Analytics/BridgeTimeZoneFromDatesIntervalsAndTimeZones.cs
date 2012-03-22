using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class BridgeTimeZoneFromDatesIntervalsAndTimeZones : IStatisticsDataSetup
	{
		public void Apply(SqlConnection connection, CultureInfo statisticsDataCulture)
		{

			// propably have to fetch using base class or interface when we need more dates and time zones
			// but not even sure this class should depend on the "current" user factory, maybe it needs to be injected
			// there's also a temporal coupling here. other setups need to be run before this one..
			var dim_date = UserFactory.User().UserData<TodayDate>().Table;
			var dim_interval = UserFactory.User().UserData<QuarterOfAnHourInterval>().Table;
			var dim_time_zone = UserFactory.User().UserData<UtcAndCetTimeZones>().Table;

			var table = bridge_time_zone.CreateTable();

			var query = from d in dim_date.AsEnumerable()
			            let date_id = (int) d["date_id"]
			            from i in dim_interval.AsEnumerable()
			            let interval_id = (int) i["interval_id"]
			            from z in dim_time_zone.AsEnumerable()
			            let time_zone_id = (int) z["time_zone_id"]
			            let timeZone = TimeZoneInfo.FindSystemTimeZoneById(z["time_zone_code"].ToString())
			            let date = (DateTime) d["date_date"]
			            let time = (DateTime) i["interval_start"]
			            let dateTime = date.Date + time.TimeOfDay
			            let localDateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, timeZone)
			            let local_dates = (
			                              	from ld in dim_date.AsEnumerable()
			                              	let ldDate = (DateTime) ld["date_date"]
			                              	where ldDate == localDateTime.Date
			                              	select (int) ld["date_id"]
			                              ).ToArray()
			            where local_dates.Any()
			            let local_date_id = local_dates.Single() 
			            let local_interval_id = (
			                                    	from li in dim_interval.AsEnumerable()
			                                    	let liTime = ((DateTime) li["interval_start"]).TimeOfDay
			                                    	where liTime == localDateTime.TimeOfDay
			                                    	select (int) li["interval_id"]
			                                    ).Single()
			            select new
			                   	{
			                   		date_id,
			                   		interval_id,
			                   		time_zone_id,
			                   		local_date_id,
			                   		local_interval_id
			                   	};

			query.ForEach(
				a => table.AddRow(
					a.date_id,
					a.interval_id,
					a.time_zone_id,
					a.local_date_id,
					a.local_interval_id,
					1,
					DateTime.Now,
					DateTime.Now)
				);

			Bulk.Insert(connection, table);
		}
	}

}