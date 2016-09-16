using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ResourceCalculateDaysDecider : IResourceCalculateDaysDecider
	{
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly IsNightShift _isNightShift;

		public ResourceCalculateDaysDecider(ITimeZoneGuard timeZoneGuard, IsNightShift isNightShift)
		{
			_timeZoneGuard = timeZoneGuard;
			_isNightShift = isNightShift;
		}

		public IList<DateOnly> DecideDates(IScheduleDay currentSchedule, IScheduleDay previousSchedule)
		{
			var current = currentSchedule.SignificantPart();
			var previous = previousSchedule.SignificantPart();
			
			if (!currentSchedule.IsScheduled() && previous == SchedulePartView.DayOff)
				return new List<DateOnly>();

			var currentDate = earliestShiftStartInUserViewPoint(previousSchedule, currentSchedule);
			if (current == SchedulePartView.DayOff && previous == SchedulePartView.MainShift)
			{
				if (!_isNightShift.Check(previousSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (current == SchedulePartView.MainShift && previous == SchedulePartView.DayOff)
			{
				if (!_isNightShift.Check(currentSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (current == SchedulePartView.MainShift && previous == SchedulePartView.MainShift)
			{
				if (!_isNightShift.Check(previousSchedule) && !_isNightShift.Check(currentSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (!currentSchedule.IsScheduled() && previous == SchedulePartView.MainShift)
			{
				if (!_isNightShift.Check(previousSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			IList<DateOnly> ret = new List<DateOnly> { currentDate, currentDate.AddDays(1) };

			return ret;
		}

		private DateOnly earliestShiftStartInUserViewPoint(IScheduleDay previous, IScheduleDay current)
		{
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			var earliestDate = previous.DateOnlyAsPeriod.DateOnly;
			var assPrevious = previous.PersonAssignment(true);	
			var visualLayersPrevious = assPrevious.ProjectionService().CreateProjection();	
			var periodPrevious = visualLayersPrevious.Period();	
			var assCurrent = current.PersonAssignment(true);
			var visualLayersCurrent = assCurrent.ProjectionService().CreateProjection();
			var periodCurrent = visualLayersCurrent.Period();

			if (periodPrevious != null)
				earliestDate = new DateOnly(periodPrevious.Value.StartDateTimeLocal(currentTimeZone));

			if (periodCurrent == null) return earliestDate;

			var currentDate = new DateOnly(periodCurrent.Value.StartDateTimeLocal(currentTimeZone));

			if (currentDate < earliestDate)
				earliestDate = currentDate;

			return earliestDate;
		}
	}
}