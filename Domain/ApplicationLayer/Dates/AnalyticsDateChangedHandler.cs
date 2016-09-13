using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Dates
{
	public class AnalyticsDateChangedHandler : 
		IHandleEvent<AnalyticsDatesChangedEvent>, 
		IRunOnHangfire
	{
		private readonly IAnalyticsIntervalRepository _analyticsIntervalRepository;
		private readonly IAnalyticsDateRepository _analyticsDateRepository;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;
		private readonly IAnalyticsBridgeTimeZoneRepository _analyticsBridgeTimeZoneRepository;

		public AnalyticsDateChangedHandler(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsIntervalRepository analyticsIntervalRepository, IAnalyticsTimeZoneRepository analyticsTimeZoneRepository, IAnalyticsBridgeTimeZoneRepository analyticsBridgeTimeZoneRepository)
		{
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsIntervalRepository = analyticsIntervalRepository;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_analyticsBridgeTimeZoneRepository = analyticsBridgeTimeZoneRepository;
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(AnalyticsDatesChangedEvent @event)
		{
			// Find all dates, intervals and timezones
			var dates = _analyticsDateRepository.GetAll();
			var dateDictionary = dates.ToDictionary(x => x.DateDate, x => x);
			var timezones = _analyticsTimeZoneRepository.GetAll();
			var intervals = _analyticsIntervalRepository.GetAll();
			var intervalDictionary = intervals.ToDictionary(x => x.Offset, x => x);

			// for each timezone, get existing entries from bridge and add/update
			foreach (var timezone in timezones)
			{
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone.TimeZoneCode);
				var existingBridges = _analyticsBridgeTimeZoneRepository
					.GetBridges(timezone.TimeZoneId)
					.ToDictionary(x => new bridgeKey(x.TimeZoneId, x.DateId, x.IntervalId), x => x);
				var toBeAdded = new List<AnalyticsBridgeTimeZone>();
				foreach (var date in dates)
				{
					var localDate = TimeZoneInfo.ConvertTimeFromUtc(date.DateDate, timeZoneInfo);
					foreach (var interval in intervals)
					{
						if (existingBridges.ContainsKey(new bridgeKey(timezone.TimeZoneId, date.DateId, interval.IntervalId)))
						{
							// For now assume we do not need to change anything
						}
						else
						{
							var localTime = localDate + interval.Offset;
							var bridge = AnalyticsBridgeTimeZone.Create(date.DateId, interval.IntervalId, timezone.TimeZoneId,
								intervalDictionary, dateDictionary, localTime);

							if (bridge == null) continue;
							toBeAdded.Add(bridge);
						}
					}
				}
				_analyticsBridgeTimeZoneRepository.Save(toBeAdded);
			}
		}

		private class bridgeKey
		{
			private int timeZoneId { get; set; }
			private int dateId { get; set; }
			private int intervalId { get; set; }

			public bridgeKey(int timezoneid, int dateid, int intervalid)
			{
				timeZoneId = timezoneid;
				dateId = dateid;
				intervalId = intervalid;
			}

			public override bool Equals(object obj)
			{
				var analyticsPermission = obj as bridgeKey;
				if (analyticsPermission == null)
					return false;
				return dateId == analyticsPermission.dateId
					   && intervalId == analyticsPermission.intervalId
					   && timeZoneId == analyticsPermission.timeZoneId;
			}
			public override int GetHashCode()
			{
				return $"{dateId}|{intervalId}|{timeZoneId}".GetHashCode();
			}
		}
	}
}