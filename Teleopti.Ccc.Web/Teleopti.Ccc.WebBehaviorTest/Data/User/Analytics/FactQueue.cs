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

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Analytics
{
	public class FactQueue : IAnalyticsDataSetup
	{
		private readonly IDateData _dates;
		private readonly IIntervalData _intervals;
		private readonly IQueueData _queue;
		private readonly IDatasourceData _datasource;
		private readonly ITimeZoneData _timeZones;
		private readonly IBridgeTimeZone _bridgeTimeZone;

		public FactQueue(IDateData dates, IIntervalData intervals, IQueueData queue, IDatasourceData datasource, ITimeZoneData timeZones, IBridgeTimeZone bridgeTimeZone)
		{
			_dates = dates;
			_intervals = intervals;
			_queue = queue;
			_datasource = datasource;
			_timeZones = timeZones;
			_bridgeTimeZone = bridgeTimeZone;
		}

		public void Apply(SqlConnection connection, CultureInfo analyticsDataCulture) {

			var table = fact_queue.CreateTable();

			var dates = _dates.Rows;
			var intervals = _intervals.Rows;
			var queue = _queue.Rows;
			var datasource = _datasource.Rows;

			var query = from d in dates
			            from i in intervals
			            from q in queue
			            let date_id = (int) d["date_id"]
			            let interval_id = (int) i["interval_id"]
			            let queue_id = (int) q["queue_id"]
			            let datasource_id = (int) q["datasource_id"]
			            let time_zone_id = datasource.FindTimeZoneIdByDatasourceId(datasource_id)
			            let bridgeTimeZones = _bridgeTimeZone.Rows.FindBridgeTimeZoneRowsByIds(date_id, interval_id, time_zone_id)
						where bridgeTimeZones.Any()
						let bridgeTimeZone = bridgeTimeZones.Single()
			            let local_date_id = (int) bridgeTimeZone["local_date_id"]
			            let local_interval_id = (int) bridgeTimeZone["local_interval_id"]
			            select new
			                   	{
			                   		date_id,
			                   		interval_id,
			                   		queue_id,
			                   		local_date_id,
			                   		local_interval_id,
			                   		datasource_id
			                   	};

			query.ForEach(a =>
			              	{
			              		var date = dates.FindDateByDateId(a.date_id);
			              		var time = intervals.FindTimeByIntervalId(a.interval_id);
			              		var dateTime = date.Add(time.TimeOfDay);
			              		var offered_calls = GenerateOfferedCalls(dateTime);
			              		var answered_calls = (offered_calls - 7).Noise(3).Abs();
			              		var abandoned_calls = (offered_calls - answered_calls/3).Noise(2).Abs();
								var talk_time_s = (offered_calls * 100).Noise(100).Abs();
								var speed_of_answer = (talk_time_s / 2).Noise(100).Abs();
			              		table.AddFact(
			              			a.date_id,
			              			a.interval_id,
			              			a.queue_id,
			              			a.local_date_id,
			              			a.local_interval_id,
			              			offered_calls,
			              			answered_calls,
			              			0,
									abandoned_calls,
			              			0,
			              			0,
			              			1.Noise(1),
									1.Noise(1),
									talk_time_s,
			              			0,
			              			talk_time_s,
			              			speed_of_answer,
			              			200.Noise(100),
									200.Noise(100),
									200.Noise(100),
									a.datasource_id
			              			);
			              	});

			Bulk.Insert(connection, table);
		}


		private static readonly IEnumerable<Tuple<TimeSpan, int>> _offeredCallsLookup =
			new[]
				{
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(0), 0),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(6), 0),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(7), 10),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(8), 30),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(9), 40),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(10), 50),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(13), 30),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(17), 50),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(20), 35),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(21), 0),
					new Tuple<TimeSpan, int>(TimeSpan.FromHours(25), 0),
				};

		private int GenerateOfferedCalls(DateTime dateTime)
		{
			var baseValue = _offeredCallsLookup
				.SkipWhile(t => t.Item1 < dateTime.TimeOfDay)
				.Select(t => t.Item2)
				.First()
				;
			var value = baseValue.Noise(5).Abs();
			return value;
		}
	}

	public static class GenerateStatisticsExtensions
	{
		private static readonly Random _random = new Random();

		public static int Noise(this int value, int vary)
		{
			var noise = _random.Next(-vary, vary);
			var newValue = value + noise;
			return newValue;
		}

		public static int Abs(this int value)
		{
			return Math.Abs(value);
		}

	}
}