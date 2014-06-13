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
		private readonly ISchedulingResultStateHolder _resultStateHolder;
		private readonly IPersonalShiftMeetingTimeChecker _personalShiftMeetingTimeChecker;

		public PersonalShiftsShiftFilter(ISchedulingResultStateHolder resultStateHolder, IPersonalShiftMeetingTimeChecker personalShiftMeetingTimeChecker)
		{
			_resultStateHolder = resultStateHolder;
			_personalShiftMeetingTimeChecker = personalShiftMeetingTimeChecker;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2")]
		public IList<IShiftProjectionCache> Filter(DateOnly dateOnly, IPerson person, IList<IShiftProjectionCache> shiftList, IWorkShiftFinderResult finderResult)
		{
			if (shiftList == null) return null;
			if (shiftList.Count == 0) return shiftList;

			var schedulePart = _resultStateHolder.Schedules[person].ScheduledDay(dateOnly);
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
					var movedShift = mainShift.MoveTo(new DateOnly(shiftProjectionCache.SchedulingDate.Date), dateOnly);
					if (!_personalShiftMeetingTimeChecker.CheckTimeMeeting(movedShift, meetings))
						continue;

					if (!_personalShiftMeetingTimeChecker.CheckTimePersonAssignment(movedShift, personalAssignment))
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
			if (schedulePart.PersonMeetingCollection().Count == 0 && ass==null)
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

			if (ass!=null)
			{
				foreach (var layer in ass.PersonalActivities())
				{
					if (!period.HasValue)
						period = layer.Period;
					else
						period = period.Value.MaximumPeriod(layer.Period);
				}
			}

			if (!period.HasValue)
				return null;

			return period.Value.TimePeriod(TimeZoneInfo.Utc);
		}
	}
}
