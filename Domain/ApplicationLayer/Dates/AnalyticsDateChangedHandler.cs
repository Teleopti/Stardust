using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.DistributedLock;
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
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;

		public AnalyticsDateChangedHandler(IAnalyticsDateRepository analyticsDateRepository, IAnalyticsIntervalRepository analyticsIntervalRepository, IAnalyticsTimeZoneRepository analyticsTimeZoneRepository, IAnalyticsBridgeTimeZoneRepository analyticsBridgeTimeZoneRepository, IDistributedLockAcquirer distributedLockAcquirer)
		{
			_analyticsDateRepository = analyticsDateRepository;
			_analyticsIntervalRepository = analyticsIntervalRepository;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;
			_analyticsBridgeTimeZoneRepository = analyticsBridgeTimeZoneRepository;
			_distributedLockAcquirer = distributedLockAcquirer;
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(AnalyticsDatesChangedEvent @event)
		{
			// Use a distributed lock to make other events trying to update at the same time fail
			_distributedLockAcquirer.TryLockForTypeOf(this, updateBridges);
		}

		private void updateBridges()
		{
			// Find all dates, intervals and timezones
			var dates = _analyticsDateRepository.GetAllPartial();
			var dateDictionary = dates.ToDictionary(x => x.DateDate, x => x.DateId);
			var timezones = _analyticsTimeZoneRepository.GetAll();
			var intervals = _analyticsIntervalRepository.GetAll();
			var intervalDictionary = intervals.ToDictionary(x => x.Offset, x => x.IntervalId);

			// for each timezone, get existing entries from bridge and add/update
			foreach (var timezone in timezones)
			{
				var maxExistingDateId = _analyticsBridgeTimeZoneRepository.GetMaxDateForTimeZone(timezone.TimeZoneId);
				var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone.TimeZoneCode);
				var existingBridges = new HashSet<AnalyticsBridgeTimeZonePartial>(_analyticsBridgeTimeZoneRepository
					.GetBridgesPartial(timezone.TimeZoneId, maxExistingDateId));
				var toBeAdded = new List<AnalyticsBridgeTimeZone>();
				foreach (var date in dates.Where(x => x.DateId >= maxExistingDateId))
				{
					foreach (var interval in intervals)
					{
						var bridge = new AnalyticsBridgeTimeZone(date.DateId, interval.IntervalId, timezone.TimeZoneId);
						if (existingBridges.Contains(bridge))
						{
							// For now assume we do not need to change anything
						}
						else
						{
							var localTime = TimeZoneInfo.ConvertTimeFromUtc(date.DateDate + interval.Offset, timeZoneInfo);
							var acceptable = bridge.FillLocals(intervalDictionary, dateDictionary, localTime);
							if (!acceptable)
								continue; // If we can't set a local date or interval because they don't exist, ignore this item
							toBeAdded.Add(bridge);
						}
					}
				}
				_analyticsBridgeTimeZoneRepository.Save(toBeAdded);
			}
		}
	}
}