using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftFilters
{
	public interface ITimeLimitsRestrictionShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList,
															IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult);
	}

	public class TimeLimitsRestrictionShiftFilter : ITimeLimitsRestrictionShiftFilter
	{
		private readonly ITimeLimitationShiftFilter _timeLimitationShiftFilter;
		private readonly ILatestStartTimeLimitationShiftFilter _latestStartTimeLimitationShiftFilter;
		private readonly IEarliestEndTimeLimitationShiftFilter _earliestEndTimeLimitationShiftFilter;

		public TimeLimitsRestrictionShiftFilter(ITimeLimitationShiftFilter timeLimitationShiftFilter,
			ILatestStartTimeLimitationShiftFilter latestStartTimeLimitationShiftFilter,
			IEarliestEndTimeLimitationShiftFilter earliestEndTimeLimitationShiftFilter)
		{
			_timeLimitationShiftFilter = timeLimitationShiftFilter;
			_latestStartTimeLimitationShiftFilter = latestStartTimeLimitationShiftFilter;
			_earliestEndTimeLimitationShiftFilter = earliestEndTimeLimitationShiftFilter;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly scheduleDayDateOnly, TimeZoneInfo agentTimeZone, IList<IShiftProjectionCache> shiftList,
																  IEffectiveRestriction restriction, IWorkShiftFinderResult finderResult)
		{
			if (shiftList.Count == 0)
				return shiftList;
			var workShiftsWithinPeriod = shiftList;

			if (restriction.StartTimeLimitation.HasValue() || restriction.EndTimeLimitation.HasValue())
			{
				var startStart = TimeSpan.Zero;
				if (restriction.StartTimeLimitation.StartTime.HasValue)
					startStart = restriction.StartTimeLimitation.StartTime.Value;

				var endEnd = restriction.EndTimeLimitation.EndTime.GetValueOrDefault(new TimeSpan(1, 23, 59, 59));

				if (restriction.StartTimeLimitation.StartTime.HasValue || restriction.EndTimeLimitation.EndTime.HasValue)
				{
					var validPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(startStart), agentTimeZone), TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(endEnd), agentTimeZone));
					workShiftsWithinPeriod = _timeLimitationShiftFilter.Filter(workShiftsWithinPeriod, validPeriod, finderResult);
				}

				if (restriction.StartTimeLimitation.EndTime.HasValue)
				{
					workShiftsWithinPeriod = _latestStartTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.StartTimeLimitation.EndTime.Value), agentTimeZone), finderResult);
				}

				if (restriction.EndTimeLimitation.StartTime.HasValue)
				{
					workShiftsWithinPeriod = _earliestEndTimeLimitationShiftFilter.Filter(workShiftsWithinPeriod, TimeZoneHelper.ConvertToUtc(scheduleDayDateOnly.Date.Add(restriction.EndTimeLimitation.StartTime.Value), agentTimeZone), finderResult);
				}
			}
			return workShiftsWithinPeriod;
		}
	}
}