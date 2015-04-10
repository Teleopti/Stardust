using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftAndMeetingFilter
	{
		IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IScheduleDay schedulePart, IWorkShiftFinderResult finderResult);
	}

	public class PersonalShiftAndMeetingFilter : IPersonalShiftAndMeetingFilter
	{
		private readonly Func<ISchedulerStateHolder> _schedulerStateHolder;

		public PersonalShiftAndMeetingFilter(Func<ISchedulerStateHolder> schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public IList<IShiftProjectionCache> Filter(IList<IShiftProjectionCache> shiftList, IScheduleDay schedulePart, IWorkShiftFinderResult finderResult)
		{
			if (shiftList.Count == 0)
				return shiftList;
			DateTimePeriod? period = GetMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (period.HasValue)
			{
				var meetings = schedulePart.PersonMeetingCollection();
				var personalAssignment = schedulePart.PersonAssignment();
				int cntBefore = shiftList.Count;
				IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
				foreach (IShiftProjectionCache t in shiftList)
				{
					IShiftProjectionCache proj = t;
					if (!proj.MainShiftProjection.Period().HasValue) continue;
					DateTimePeriod virtualPeriod = proj.MainShiftProjection.Period().Value;

					if (virtualPeriod.Contains(period.Value) && t.PersonalShiftsAndMeetingsAreInWorkTime(meetings, personalAssignment))
					{
						workShiftsWithinPeriod.Add(proj);
					}
				}
				var currentTimeZone = _schedulerStateHolder().TimeZoneInfo;
				finderResult.AddFilterResults(
					new WorkShiftFilterResult(
						string.Format(CultureInfo.InvariantCulture,
									  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
									  period.Value.StartDateTimeLocal(currentTimeZone), period.Value.EndDateTimeLocal(currentTimeZone)), cntBefore,
						workShiftsWithinPeriod.Count));

				return workShiftsWithinPeriod;

			}
			return shiftList;
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