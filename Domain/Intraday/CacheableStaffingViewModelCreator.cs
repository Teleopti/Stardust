using System;
using System.Linq;
using System.Runtime.Caching;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CacheableStaffingViewModelCreator : ICacheableStaffingViewModelCreator
	{
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private const double tolerance = 0.00001;

		public CacheableStaffingViewModelCreator(IStaffingViewModelCreator staffingViewModelCreator)
		{
			_staffingViewModelCreator = staffingViewModelCreator;
		}

		public IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage)
		{
			var cacheKey = getCacheKey(skillId, useShrinkage);
			var intradyStaffingViewModelCache = MemoryCache.Default.Get(cacheKey) as IntradayStaffingViewModel;
			if (intradyStaffingViewModelCache != null)
				return intradyStaffingViewModelCache;

			var intradyStaffingViewModel = _staffingViewModelCreator.Load(new[] {skillId}, useShrinkage);
			if (!existsStaffingData(intradyStaffingViewModel))
				return intradyStaffingViewModel;
			var cachePolicy = new CacheItemPolicy {SlidingExpiration = new TimeSpan(0, 10, 0)};
			MemoryCache.Default.Set(cacheKey, intradyStaffingViewModel, cachePolicy);
			return intradyStaffingViewModel;
		}

		private string getCacheKey(Guid skillId, bool useShrinkage)
		{
			return $"{skillId}_{useShrinkage}";
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
	}

	public interface ICacheableStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage);
	}
}
