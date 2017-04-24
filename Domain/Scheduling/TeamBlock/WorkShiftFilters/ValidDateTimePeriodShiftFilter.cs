using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IValidDateTimePeriodShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult);
	}

	public class ValidDateTimePeriodShiftFilter : IValidDateTimePeriodShiftFilter
	{
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly IUserCulture _userCulture;

		public ValidDateTimePeriodShiftFilter(ITimeZoneGuard timeZoneGuard, IUserCulture userCulture)
		{
			_timeZoneGuard = timeZoneGuard;
			_userCulture = userCulture;
		}

		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, DateTimePeriod validPeriod, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (finderResult == null) return null;
		    if (shiftList.Count == 0) return shiftList;
			var cntBefore = shiftList.Count;
			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new {s, OuterPeriod = s.TheMainShift.LayerCollection.OuterPeriod()})
					.Where(s => s.OuterPeriod.HasValue && validPeriod.Contains(s.OuterPeriod.Value))
					.Select(s => s.s)
					.ToList();

			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(_userCulture.GetCulture(),
								  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
								  validPeriod.StartDateTimeLocal(currentTimeZone), validPeriod.EndDateTimeLocal(currentTimeZone)), cntBefore,
					workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}
	}
}
