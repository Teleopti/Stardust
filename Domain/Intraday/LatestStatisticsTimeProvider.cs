using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class LatestStatisticsTimeProvider
	{
		private readonly ILatestStatisticsIntervalIdLoader _latestStatisticsIntervalIdLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private readonly IUserTimeZone _userTimeZone;
		private readonly INow _now;

		public LatestStatisticsTimeProvider(
			ILatestStatisticsIntervalIdLoader latestStatisticsIntervalIdLoader, 
			IIntervalLengthFetcher intervalLengthFetcher,
			IUserTimeZone userTimeZone,
			INow now)
		{
			_latestStatisticsIntervalIdLoader = latestStatisticsIntervalIdLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
			_userTimeZone = userTimeZone;
			_now = now;
		}

		public LatestStatitsticsTimeModel Get(Guid[] skillIdList)
		{
			return Get(skillIdList, _now.UtcDateTime());
		}

		public LatestStatitsticsTimeModel Get(Guid[] skillIdList, int dayOffset)
		{
			return Get(skillIdList, _now.UtcDateTime().AddDays(dayOffset));
		}

		public LatestStatitsticsTimeModel Get(Guid[] skillIdList, DateTime dateUtc)
		{
			var userTime = new DateOnly(TimeZoneHelper.ConvertFromUtc(dateUtc, _userTimeZone.TimeZone()));
			var intervalId = _latestStatisticsIntervalIdLoader.Load(skillIdList, userTime, _userTimeZone.TimeZone());
			if (!intervalId.HasValue)
				return null;

			int minutesPerInterval = _intervalLengthFetcher.IntervalLength;

			return new LatestStatitsticsTimeModel
			{
				StartTime = DateTime.MinValue.AddMinutes(intervalId.Value * minutesPerInterval),
				EndTime = DateTime.MinValue.AddMinutes((intervalId.Value + 1) * minutesPerInterval)
			};
		}


	}
}