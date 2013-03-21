using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface ITimeLimitsRestrictionShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, IPerson person, IList<IShiftProjectionCache> shiftList,
															IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
	}

	public class TimeLimitsRestrictionShiftFilter : ITimeLimitsRestrictionShiftFilter
	{
		private readonly IValidDateTimePeriodShiftFilter _validDateTimePeriodShiftFilter;
		private readonly ILatestStartTimeLimitationShiftFilter _latestStartTimeLimitationShiftFilter;
		private readonly IEarliestEndTimeLimitationShiftFilter _earliestEndTimeLimitationShiftFilter;

		public TimeLimitsRestrictionShiftFilter(IValidDateTimePeriodShiftFilter validDateTimePeriodShiftFilter,
			ILatestStartTimeLimitationShiftFilter latestStartTimeLimitationShiftFilter,
			IEarliestEndTimeLimitationShiftFilter earliestEndTimeLimitationShiftFilter)
		{
			_validDateTimePeriodShiftFilter = validDateTimePeriodShiftFilter;
			_latestStartTimeLimitationShiftFilter = latestStartTimeLimitationShiftFilter;
			_earliestEndTimeLimitationShiftFilter = earliestEndTimeLimitationShiftFilter;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, IPerson person, IList<IShiftProjectionCache> shiftList,
																  IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
		{
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
					workShiftsWithinPeriod = _validDateTimePeriodShiftFilter.Filter(workShiftsWithinPeriod, validPeriod, finderResult);
				}

				if (restriction.StartTimeLimitation.EndTime.HasValue)
				{
					workShiftsWithinPeriod = _latestStartTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.StartTimeLimitation.EndTime.Value), timeZone), finderResult);
				}

				if (restriction.EndTimeLimitation.StartTime.HasValue)
				{
					workShiftsWithinPeriod = _earliestEndTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.EndTimeLimitation.StartTime.Value), timeZone), finderResult);
				}
			}
			return workShiftsWithinPeriod;
		}
	}
}