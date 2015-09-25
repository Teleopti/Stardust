using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftFilters
{
	public interface IPersonalShiftsShiftFilter
	{
		IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IShiftProjectionCache> shiftList,IWorkShiftFinderResult finderResult);
	}
	
	public class PersonalShiftsShiftFilter : IPersonalShiftsShiftFilter
	{
		private readonly Func<ISchedulingResultStateHolder> _resultStateHolder;
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public PersonalShiftsShiftFilter(Func<ISchedulingResultStateHolder> resultStateHolder, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_resultStateHolder = resultStateHolder;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}

		public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var schedulePart = _resultStateHolder().Schedules[person].ScheduledDay(dateOnly);
			TimePeriod? period = getMaximumPeriodForPersonalShiftsAndMeetings(schedulePart);
			if (period.HasValue)
			{
				var meetings = schedulePart.PersonMeetingCollection();
				var personalAssignment = schedulePart.PersonAssignment();
				IList<IShiftProjectionCache> workShiftsWithinPeriod = new List<IShiftProjectionCache>();
				foreach (var shiftProjectionCache in shiftList)
				{
					var currentPeriod = shiftProjectionCache.MainShiftProjection.Period();
					if (!currentPeriod.HasValue) 
						continue;

					TimePeriod virtualPeriod = shiftProjectionCache.MainShiftProjection.Period().Value.TimePeriod(TimeZoneInfo.Utc);
					if (!virtualPeriod.Contains(period.Value))
						continue;

					var mainShift = shiftProjectionCache.TheMainShift;
					var movedShift = mainShift.MoveTo(shiftProjectionCache.SchedulingDate, dateOnly);

					if (!_personalShiftMeetingTimeChecker.CheckTimeMeeting(movedShift, meetings))
						continue;

					if (personalAssignment != null && !_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(movedShift, personalAssignment))
						continue;

					workShiftsWithinPeriod.Add(shiftProjectionCache);
				}

				return workShiftsWithinPeriod;

			}

			return shiftList;
		}

		private static TimePeriod? getMaximumPeriodForPersonalShiftsAndMeetings(IScheduleDay schedulePart)
		{
			var ass = schedulePart.PersonAssignment();
			var meetings = schedulePart.PersonMeetingCollection();
			if (meetings.Count == 0 && ass==null)
			{
				return null;
			}

			DateTimePeriod? period = null;

			foreach (IPersonMeeting personMeeting in meetings)
			{
				period = !period.HasValue ? personMeeting.Period : period.Value.MaximumPeriod(personMeeting.Period);
			}

			if (ass!=null)
			{
				foreach (var layer in ass.PersonalActivities())
				{
					period = !period.HasValue ? layer.Period : period.Value.MaximumPeriod(layer.Period);
				}
			}

			if (!period.HasValue)
				return null;

			return period.Value.TimePeriod(TimeZoneInfo.Utc);
		}
	}
}
