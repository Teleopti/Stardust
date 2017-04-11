using System;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CacheableStaffingViewModelCreator : ICacheableStaffingViewModelCreator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(CacheableStaffingViewModelCreator));
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IIntervalLengthFetcher _intervalLengthFetcher;
		private const double tolerance = 0.00001;

		public CacheableStaffingViewModelCreator(IStaffingViewModelCreator staffingViewModelCreator, IIntervalLengthFetcher intervalLengthFetcher)
		{
			_staffingViewModelCreator = staffingViewModelCreator;
			_intervalLengthFetcher = intervalLengthFetcher;
		}

		public IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage)
		{
			var cacheKey = getCacheKey(skillId, useShrinkage);
			var intradyStaffingViewModelCache = loadDataFromCache(cacheKey);
			if (intradyStaffingViewModelCache != null)
				return intradyStaffingViewModelCache;

			var intradyStaffingViewModel = _staffingViewModelCreator.Load(new[] {skillId}, null, useShrinkage);
			logIntradayStaffingViewModel(skillId, useShrinkage, intradyStaffingViewModel);
			if (!existsStaffingData(intradyStaffingViewModel))
				return intradyStaffingViewModel;

			cacheData(cacheKey, intradyStaffingViewModel);
			return intradyStaffingViewModel;
		}

		private static void cacheData(string cacheKey, IntradayStaffingViewModel intradyStaffingViewModel)
		{
			var cachePolicy = new CacheItemPolicy {SlidingExpiration = new TimeSpan(0, 10, 0)};
			MemoryCache.Default.Set(cacheKey, intradyStaffingViewModel, cachePolicy);
		}

		private static IntradayStaffingViewModel loadDataFromCache(string cacheKey)
		{
			var intradyStaffingViewModelCache = MemoryCache.Default.Get(cacheKey) as IntradayStaffingViewModel;
			return intradyStaffingViewModelCache;
		}

		private string getCacheKey(Guid skillId, bool useShrinkage)
		{
			return $"{skillId}_{useShrinkage}_{_intervalLengthFetcher.IntervalLength}";
		}

		private bool existsStaffingData(IntradayStaffingViewModel intradyStaffingViewModel)
		{
			if (!intradyStaffingViewModel.StaffingHasData)
				return false;

			if (intradyStaffingViewModel.DataSeries.ScheduledStaffing == null
				|| intradyStaffingViewModel.DataSeries.ScheduledStaffing?.Length == 0)
				return false;

			if (intradyStaffingViewModel.DataSeries.ScheduledStaffing.All(s => Math.Abs(s.GetValueOrDefault()) < tolerance))
				return false;

			if (intradyStaffingViewModel.DataSeries.ForecastedStaffing.All(s => Math.Abs(s.GetValueOrDefault()) < tolerance))
				return false;

			return true;
		}

		private void logIntradayStaffingViewModel(Guid skillId, bool useShrinkage,
			IntradayStaffingViewModel intradayStaffingViewModel)
		{
			if (intradayStaffingViewModel.DataSeries?.Time == null)
			{
				logger.Warn($"no data for {skillId}");
				return;
			}
				
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"Skill:{skillId}, useShrinkage:{useShrinkage}");
			for (int i = 0; i < intradayStaffingViewModel.DataSeries.Time.Length; i++)
			{
				stringBuilder.Append($"[{intradayStaffingViewModel.DataSeries.Time[i]},");
				stringBuilder.Append(i <= intradayStaffingViewModel.DataSeries.ScheduledStaffing?.Length - 1
					? $"{intradayStaffingViewModel.DataSeries.ScheduledStaffing[i] ?? double.NaN},"
					: "NotFound");
				stringBuilder.Append(i <= intradayStaffingViewModel.DataSeries.ForecastedStaffing?.Length - 1
					? $"{intradayStaffingViewModel.DataSeries.ForecastedStaffing[i] ?? double.NaN}"
					: "NotFound");
				stringBuilder.Append("],");
			}
			stringBuilder.AppendLine();
			logger.Warn(stringBuilder.ToString());
		}
	}

	public interface ICacheableStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage);
	}
}
