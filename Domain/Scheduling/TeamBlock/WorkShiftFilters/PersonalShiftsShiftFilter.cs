using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftsShiftFilter
	{
		IList<ShiftProjectionCache> Filter(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IPerson person, IList<ShiftProjectionCache> shiftList,WorkShiftFinderResult finderResult);
	}
	
	public class PersonalShiftsShiftFilter : IPersonalShiftsShiftFilter
	{
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public PersonalShiftsShiftFilter(IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}

		public IList<ShiftProjectionCache> Filter(IScheduleDictionary scheduleDictionary, DateOnly dateOnly, IPerson person, IList<ShiftProjectionCache> shiftList, WorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var schedulePart = scheduleDictionary[person].ScheduledDay(dateOnly);
			var meetings = schedulePart.PersonMeetingCollection();
			var personalAssignment = schedulePart.PersonAssignment();

			TimePeriod? period = getMaximumPeriodForPersonalShiftsAndMeetings(personalAssignment,meetings);
			if (!period.HasValue) return shiftList;

			return shiftList.Select(
					shiftProjectionCache =>
							new {shiftProjectionCache, currentPeriod = shiftProjectionCache.MainShiftProjection.Period()})
				.Where(t => t.currentPeriod.HasValue && t.currentPeriod.Value.TimePeriod(TimeZoneInfo.Utc).Contains(period.Value))
				.Select(t => new {t, movedShift = t.shiftProjectionCache.TheMainShift.MoveTo(t.shiftProjectionCache.SchedulingDate, dateOnly)})
				.Where(t => _personalShiftMeetingTimeChecker.CheckTimeMeeting(t.movedShift, meetings) && (personalAssignment == null || _personalShiftMeetingTimeChecker.CheckTimePersonAssignment(t.movedShift, personalAssignment)))
				.Select(t => t.t.shiftProjectionCache).ToList();
		}

		private static TimePeriod? getMaximumPeriodForPersonalShiftsAndMeetings(IPersonAssignment ass, ReadOnlyCollection<IPersonMeeting> meetings)
		{
			if (meetings.Count == 0 && ass==null)
			{
				return null;
			}

			DateTimePeriod? period = null;

			foreach (IPersonMeeting personMeeting in meetings)
			{
				period = period?.MaximumPeriod(personMeeting.Period) ?? personMeeting.Period;
			}

			if (ass!=null)
			{
				foreach (var layer in ass.PersonalActivities())
				{
					period = period?.MaximumPeriod(layer.Period) ?? layer.Period;
				}
			}

			return period?.TimePeriod(TimeZoneInfo.Utc);
		}
	}
}
