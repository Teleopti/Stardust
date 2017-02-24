using System;
using System.Runtime.Caching;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CacheableStaffingViewModelCreator : ICacheableStaffingViewModelCreator
	{
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;

		public CacheableStaffingViewModelCreator(IStaffingViewModelCreator staffingViewModelCreator)
		{
			_staffingViewModelCreator = staffingViewModelCreator;
		}

		public IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage)
		{
			var cacheKey = getCacheKey(skillId);
			var intradyStaffingViewModelCache = MemoryCache.Default.Get(cacheKey) as IntradayStaffingViewModel;
			if (intradyStaffingViewModelCache != null)
				return intradyStaffingViewModelCache;

			var intradyStaffingViewModel = _staffingViewModelCreator.Load(new[] {skillId}, useShrinkage);
			if (!intradyStaffingViewModel.StaffingHasData || intradyStaffingViewModel.DataSeries.ScheduledStaffing?.Length == 0)
				return intradyStaffingViewModel;
			var cachePolicy = new CacheItemPolicy {SlidingExpiration = new TimeSpan(0, 10, 0)};
			MemoryCache.Default.Set(cacheKey, intradyStaffingViewModel, cachePolicy);
			return intradyStaffingViewModel;
		}

		private string getCacheKey(Guid skillId)
		{
			return skillId.ToString();
		}
	}

	public interface ICacheableStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid skillId, bool useShrinkage);
	}
}
