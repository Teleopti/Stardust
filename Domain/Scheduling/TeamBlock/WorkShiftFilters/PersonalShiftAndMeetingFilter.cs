using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftAndMeetingFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart);
	}

	public class PersonalShiftAndMeetingFilter : IPersonalShiftAndMeetingFilter
	{
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart)
		{
			if (shiftList.Count == 0)
				return shiftList;

			var period = GetMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (!period.HasValue) return shiftList;

			var meetings = schedulePart.PersonMeetingCollection();
			var personalAssignment = schedulePart.PersonAssignment();
			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new {s, Period = s.MainShiftProjection.Period()})
					.Where(
						s =>
							s.Period.HasValue && s.Period.Value.Contains(period.Value) &&
							s.s.PersonalShiftsAndMeetingsAreInWorkTime(meetings, personalAssignment))
					.Select(s => s.s)
					.ToList();

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