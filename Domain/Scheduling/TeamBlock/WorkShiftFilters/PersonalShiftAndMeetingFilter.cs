using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftAndMeetingFilter
	{
		IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart);
	}

	public class PersonalShiftAndMeetingFilter : IPersonalShiftAndMeetingFilter
	{
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public PersonalShiftAndMeetingFilter(IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}
		
		public IList<ShiftProjectionCache> Filter(IList<ShiftProjectionCache> shiftList, IScheduleDay schedulePart)
		{
			if (shiftList.Count == 0)
				return shiftList;

			var period = GetMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (!period.HasValue) return shiftList;

			var meetings = schedulePart.PersonMeetingCollection();
			var personalAssignment = schedulePart.PersonAssignment();
			IList<ShiftProjectionCache> workShiftsWithinPeriod =
				shiftList.Select(s => new {s, Period = s.MainShiftProjection().Period()})
					.Where(
						s =>
							s.Period.HasValue && s.Period.Value.Contains(period.Value) &&
							personalShiftsAndMeetingsAreInWorkTime(s.s.TheMainShift, meetings, personalAssignment))
					.Select(s => s.s)
					.ToList();

			return workShiftsWithinPeriod;
		}

		public DateTimePeriod? GetMaximumPeriodForPersonalShiftsAndMeetings(IScheduleDay schedulePart)
		{
			var ass = schedulePart.PersonAssignment();
			var meetings = schedulePart.PersonMeetingCollection();
			if (meetings.Length == 0 && ass == null)
			{
				return null;
			}

			DateTimePeriod? period = null;

			foreach (IPersonMeeting personMeeting in meetings)
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
		
		private bool personalShiftsAndMeetingsAreInWorkTime(IEditableShift mainShift, IPersonMeeting[] meetings, IPersonAssignment personAssignment)
		{
			if (meetings.Length == 0 && personAssignment == null)
			{
				return true;
			}

			if (meetings.Length > 0 && !_personalShiftMeetingTimeChecker.CheckTimeMeeting(mainShift, meetings))
				return false;

			if (personAssignment != null && !_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(mainShift, personAssignment))
				return false;

			return true;
		}
	}
}