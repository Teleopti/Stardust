using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public class TimeLimitsRestrictionShiftFilter
	{
		private readonly ValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILatestStartTimeLimitationShiftFilter _latestStartTimeLimitationShiftFilter;
		private readonly IEarliestEndTimeLimitationShiftFilter _earliestEndTimeLimitationShiftFilter;

		public TimeLimitsRestrictionShiftFilter(ValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
			ILatestStartTimeLimitationShiftFilter latestStartTimeLimitationShiftFilter,
			IEarliestEndTimeLimitationShiftFilter earliestEndTimeLimitationShiftFilter)
		{
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_latestStartTimeLimitationShiftFilter = latestStartTimeLimitationShiftFilter;
			_earliestEndTimeLimitationShiftFilter = earliestEndTimeLimitationShiftFilter;
		}

		public IList<ShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, IPerson person, IList<ShiftProjectionCache> shiftList, IEffectiveRestriction restriction)
		{
			if (person == null) return null;
			if (shiftList == null) return null;
			if (restriction == null) return null;
		    if (shiftList.Count == 0)
				return shiftList;
            var timeZone = person.PermissionInformation.DefaultTimeZone();

			var workShiftsWithinPeriod = shiftList;

			if (restriction.StartTimeLimitation.HasValue() || restriction.EndTimeLimitation.HasValue())
			{
				var startStart = TimeSpan.Zero;
				if (restriction.StartTimeLimitation.StartTime.HasValue)
					startStart = restriction.StartTimeLimitation.StartTime.Value;

				var endEnd = restriction.EndTimeLimitation.EndTime.GetValueOrDefault(new TimeSpan(1, 23, 59, 59));

				if (restriction.StartTimeLimitation.StartTime.HasValue || restriction.EndTimeLimitation.EndTime.HasValue)
				{
					var validPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(startStart), timeZone), TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(endEnd), timeZone));
					workShiftsWithinPeriod = _validDateTimePeriodShiftFilter.Filter(workShiftsWithinPeriod, validPeriod);
				}

				if (restriction.StartTimeLimitation.EndTime.HasValue)
				{
					workShiftsWithinPeriod = _latestStartTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.StartTimeLimitation.EndTime.Value), timeZone));
				}

				if (restriction.EndTimeLimitation.StartTime.HasValue)
				{
					workShiftsWithinPeriod = _earliestEndTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.EndTimeLimitation.StartTime.Value), timeZone));
				}
			}
			return workShiftsWithinPeriod;
		}
	}
}