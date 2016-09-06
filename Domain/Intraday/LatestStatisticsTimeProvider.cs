using System;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

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
			var usersToday = new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _userTimeZone.TimeZone()));
			var intervalId = _latestStatisticsIntervalIdLoader.Load(skillIdList, usersToday, _userTimeZone.TimeZone());
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