using System;
using System.Runtime.Caching;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class CacheableStaffingViewModelCreator : ICacheableStaffingViewModelCreator
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;

		public CacheableStaffingViewModelCreator(IStaffingViewModelCreator staffingViewModelCreator,
			ILoggedOnUser loggedOnUser)
		{
			_staffingViewModelCreator = staffingViewModelCreator;
			_loggedOnUser = loggedOnUser;
		}

		public IntradayStaffingViewModel Load(Guid skillId)
		{
			var cacheKey = getCacheKey(skillId);
			var intradyStaffingViewModelCache = MemoryCache.Default.Get(cacheKey) as IntradayStaffingViewModel;
			if (intradyStaffingViewModelCache != null)
				return intradyStaffingViewModelCache;

			var intradyStaffingViewModel = _staffingViewModelCreator.Load(new[] {skillId});
			var cachePolicy = new CacheItemPolicy {SlidingExpiration = new TimeSpan(0, 10, 0)};
			MemoryCache.Default.Set(cacheKey, intradyStaffingViewModel, cachePolicy);
			return intradyStaffingViewModel;
		}

		private string getCacheKey(Guid skillId)
		{
			return $"{_loggedOnUser.CurrentUser().Id.GetValueOrDefault()}_{skillId}";
		}
	}

	public interface ICacheableStaffingViewModelCreator
	{
		IntradayStaffingViewModel Load(Guid skillId);
	}
}
