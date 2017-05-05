using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftAndMeetingFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart, WorkShiftFinderResult finderResult);
	}

	public class PersonalShiftAndMeetingFilter : IPersonalShiftAndMeetingFilter
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public PersonalShiftAndMeetingFilter(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart, WorkShiftFinderResult finderResult)
		{
			if (shiftList.Count == 0)
				return shiftList;

			var period = GetMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (!period.HasValue) return shiftList;

			var meetings = schedulePart.PersonMeetingCollection();
			var personalAssignment = schedulePart.PersonAssignment();
			int cntBefore = shiftList.Count;
			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new {s, Period = s.MainShiftProjection.Period()})
					.Where(
						s =>
							s.Period.HasValue && s.Period.Value.Contains(period.Value) &&
							s.s.PersonalShiftsAndMeetingsAreInWorkTime(meetings, personalAssignment))
					.Select(s => s.s)
					.ToList();

			var currentTimeZone = _schedulerStateHolder().TimeZoneInfo;
			finderResult.AddFilterResults(
				new WorkShiftFilterResult(
					string.Format(CultureInfo.InvariantCulture,
						UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
						period.Value.StartDateTimeLocal(currentTimeZone), period.Value.EndDateTimeLocal(currentTimeZone)), cntBefore,
					workShiftsWithinPeriod.Count));

			return workShiftsWithinPeriod;
		}

		public DateTimePeriod? GetMaximumPeriodForPersonalShiftsAndMeetings(IScheduleDay schedulePart)
		{
			var ass = schedulePart.PersonAssignment();
			if (schedulePart.PersonMeetingCollection().Count == 0 && ass == null)
			{
				return null;
			}

			DateTimePeriod? period = null;

			foreach (IPersonMeeting personMeeting in schedulePart.PersonMeetingCollection())
			{
				if (!period.HasValue)
					period = personMeeting.Period;

				period = period.Value.MaximumPeriod(personMeeting.Period);
			}

			if (ass != null)
			{
				foreach (var personalLayer in ass.PersonalActivities())
				{
					if (!period.HasValue)
						period = personalLayer.Period;
					period = period.Value.MaximumPeriod(personalLayer.Period);
				}
			}
			return period;
		}
	}
}