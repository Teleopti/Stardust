using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftsShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IShiftProjectionCache> shiftList,IWorkShiftFinderResult finderResult);
	}
	
	public class PersonalShiftsShiftFilter : IPersonalShiftsShiftFilter
	{
		private readonly ISchedulingResultStateHolder _resultStateHolder;

		public PersonalShiftsShiftFilter(ISchedulingResultStateHolder resultStateHolder)
		{
			_resultStateHolder = resultStateHolder;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IShiftProjectionCache> shiftList,IWorkShiftFinderResult finderResult)
		{
			if (shiftList.Count == 0) return shiftList;

			var schedulePart = _resultStateHolder.Schedules[person].ScheduledDay(dateOnly);
			DateTimePeriod? period = getMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (period.HasValue)
			{
				var meetings = schedulePart.PersonMeetingCollection();
				var personalAssignments = schedulePart.PersonAssignmentCollection();
				int cntBefore = shiftList.Count;
				IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
				foreach (IShiftProjectionCache t in shiftList)
				{
					IShiftProjectionCache proj = t;
					if (!proj.MainShiftProjection.Period().HasValue) continue;
					DateTimePeriod virtualPeriod = proj.MainShiftProjection.Period().Value;

					if (virtualPeriod.Contains(period.Value) && t.PersonalShiftsAndMeetingsAreInWorkTime(meetings, personalAssignments))
					{
						workShiftsWithinPeriod.Add(proj);
					}
				}
				finderResult.AddFilterResults(
					new WorkShiftFilterResult(
						string.Format(TeleoptiPrincipal.Current.Regional.Culture,
									  UserTexts.Resources.FilterOnPersonalPeriodLimitationsWithParams,
									  period.Value.LocalStartDateTime, period.Value.LocalEndDateTime), cntBefore,
						workShiftsWithinPeriod.Count));

				return workShiftsWithinPeriod;

			}
			return shiftList;
		}

		private static DateTimePeriod? getMaximumPeriodForPersonalShiftsAndMeetings(IScheduleDay schedulePart)
		{
			if (schedulePart.PersonMeetingCollection().Count == 0 && schedulePart.PersonAssignmentCollection().Count == 0)
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

			if (schedulePart.PersonAssignmentCollection().Count > 0)
			{
				foreach (IPersonalShift personalShift in schedulePart.PersonAssignmentCollection()[0].PersonalShiftCollection)
				{
					if (!period.HasValue && personalShift.LayerCollection.Period().HasValue)
						period = personalShift.LayerCollection.Period().Value;
					if (personalShift.LayerCollection.Period().HasValue)
						if (period != null)
							period = period.Value.MaximumPeriod(personalShift.LayerCollection.Period().Value);
				}
			}
			return period;
		}
	}
}
