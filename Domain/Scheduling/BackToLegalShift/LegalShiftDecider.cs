using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.BackToLegalShift
{
	public interface ILegalShiftDecider
	{
		bool IsLegalShift(IRuleSetBag rulesetBag, ShiftProjectionCache currentShiftProjectionCache, IScheduleDay scheduleDay);
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

		public bool IsLegalShift(IRuleSetBag rulesetBag, ShiftProjectionCache currentShiftProjectionCache, IScheduleDay scheduleDay)
		{
			var personAssignment = scheduleDay.PersonAssignment(true);
			foreach (var personalShiftLayer in personAssignment.PersonalActivities())
			{
				foreach (var shiftLayer in personAssignment.ShiftLayers)
				{
					if (personalShiftLayer.Equals(shiftLayer)) continue;
					if (!shiftLayer.Payload.AllowOverwrite && personalShiftLayer.Period.Intersect(shiftLayer.Period))
						return false;
				}
			}

			foreach (var personMeeting in scheduleDay.PersonMeetingCollection())
			{
				foreach (var shiftLayer in personAssignment.ShiftLayers)
				{
					if (!shiftLayer.Payload.AllowOverwrite && personMeeting.Period.Intersect(shiftLayer.Period))
						return false;
				}
			}

			var shifts = _shiftProjectionCacheManager.ShiftProjectionCachesFromRuleSets(scheduleDay.DateOnlyAsPeriod, rulesetBag, false,
				true);

			foreach (var shiftProjectionCache in shifts)
			{
				if (
					_scheduleDayEquator.MainShiftEquals(
						shiftProjectionCache.TheWorkShift.ToEditorShift(scheduleDay.DateOnlyAsPeriod,
							scheduleDay.DateOnlyAsPeriod.TimeZone()),
						currentShiftProjectionCache.TheWorkShift.ToEditorShift(scheduleDay.DateOnlyAsPeriod,
							scheduleDay.DateOnlyAsPeriod.TimeZone())))
				return true;
			}

			return false;
		}
	}
}