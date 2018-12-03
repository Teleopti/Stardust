using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday.ApplicationLayer
{
	public interface ILatestStatisticsTimeProvider
	{
		LatestStatitsticsTimeModel Get(Guid[] skillIdList);
		LatestStatitsticsTimeModel Get(Guid[] skillIdList, int dayOffset);
		LatestStatitsticsTimeModel Get(Guid[] skillIdList, DateTime dateUtc);
	}

	public class LatestStatisticsTimeProvider : ILatestStatisticsTimeProvider
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
			var intervalId = _latestStatisticsIntervalIdLoader.Load(skillIdList, new DateOnly(dateUtc), TimeZoneInfo.Utc);
			if (!intervalId.HasValue)
				return null;

			int minutesPerInterval = _intervalLengthFetcher.IntervalLength;

			return new LatestStatitsticsTimeModel
			{
				StartTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.MinValue.AddMinutes(intervalId.Value * minutesPerInterval), _userTimeZone.TimeZone()),
				EndTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.MinValue.AddMinutes((intervalId.Value + 1) * minutesPerInterval), _userTimeZone.TimeZone())
			};
		}


	}
}