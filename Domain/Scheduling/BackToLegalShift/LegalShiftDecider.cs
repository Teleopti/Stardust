

using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface ILegalShiftDecider
	{
		bool IsLegalShift(DateOnly date, TimeZoneInfo timeZoneInfo, IRuleSetBag rulesetBag, IShiftProjectionCache currentShiftProjectionCache);
	}

	public class LegalShiftDecider : ILegalShiftDecider
	{
		private readonly IShiftProjectionCacheManager _shiftProjectionCacheManager;
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public LegalShiftDecider(IShiftProjectionCacheManager shiftProjectionCacheManager, IScheduleDayEquator scheduleDayEquator)
		{
			_shiftProjectionCacheManager = shiftProjectionCacheManager;
			_scheduleDayEquator = scheduleDayEquator;
		}

		public bool IsLegalShift(DateOnly date, TimeZoneInfo timeZoneInfo, IRuleSetBag rulesetBag, IShiftProjectionCache currentShiftProjectionCache)
		{
			var shifts = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(date, timeZoneInfo, rulesetBag, false,
				true);

			var period = new DateOnlyAsDateTimePeriod(date, timeZoneInfo);
			foreach (var shiftProjectionCach in shifts)
			{
				if (_scheduleDayEquator.MainShiftEquals(shiftProjectionCach.TheWorkShift.ToEditorShift(period, timeZoneInfo),
					currentShiftProjectionCache.TheWorkShift.ToEditorShift(period, timeZoneInfo)))
					return true;
			}

			return false;
		}
	}
}