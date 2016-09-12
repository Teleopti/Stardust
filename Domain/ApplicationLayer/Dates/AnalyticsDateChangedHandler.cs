using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure.Analytics;

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
				var existingBridges = _analyticsBridgeTimeZoneRepository.GetBridges(timezone.TimeZoneId);
				var toBeAdded = new List<AnalyticsBridgeTimeZone>();
				foreach (var date in dates)
				{
					var localDate = TimeZoneInfo.ConvertTimeFromUtc(date.DateDate, timeZoneInfo);
					foreach (var interval in intervals)
					{
						if (existingBridges.Any(x => x.DateId == date.DateId && x.IntervalId == interval.IntervalId))
						{
							// Update if needed?
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
	}
}