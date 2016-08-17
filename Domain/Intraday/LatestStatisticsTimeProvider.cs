using System;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class LatestStatisticsTimeProvider
	{
		private readonly ILatestStatisticsIntervalIdLoader _latestStatisticsIntervalIdLoader;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;

		public LatestStatisticsTimeProvider(ILatestStatisticsIntervalIdLoader latestStatisticsIntervalIdLoader, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_latestStatisticsIntervalIdLoader = latestStatisticsIntervalIdLoader;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public DateTime? Get()
		{
			var intervalId = _latestStatisticsIntervalIdLoader.Load();
			if (!intervalId.HasValue)
				return null;
			return DateTime.MinValue.AddMinutes(_latestStatisticsIntervalIdLoader.Load().Value*_intervalLengthFetcher.IntervalLength);
		}
	}
}