using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ResourceCalculateDaysDecider : IResourceCalculateDaysDecider
	{
		public IList<DateOnly> DecideDates(IScheduleDay currentSchedule, IScheduleDay previousSchedule)
		{
			var current = currentSchedule.SignificantPart();
			var previous = previousSchedule.SignificantPart();
			
			if (!currentSchedule.IsScheduled() && previous == SchedulePartView.DayOff)
				return new List<DateOnly>();

			var currentDate = earliestShiftStartInUserViewPoint(previousSchedule, currentSchedule);
			if (current == SchedulePartView.DayOff && previous == SchedulePartView.MainShift)
			{
				if (!IsNightShift(previousSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (current == SchedulePartView.MainShift && previous == SchedulePartView.DayOff)
			{
				if (!IsNightShift(currentSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (current == SchedulePartView.MainShift && previous == SchedulePartView.MainShift)
			{
				if (!IsNightShift(previousSchedule) && !IsNightShift(currentSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			if (!currentSchedule.IsScheduled() && previous == SchedulePartView.MainShift)
			{
				if (!IsNightShift(previousSchedule))
				{
					return new List<DateOnly> { currentDate };
				}
			}

			IList<DateOnly> ret = new List<DateOnly> { currentDate, currentDate.AddDays(1) };

			return ret;
		}

		private static DateOnly earliestShiftStartInUserViewPoint(IScheduleDay previous, IScheduleDay current)
		{
			var earliestDate = previous.DateOnlyAsPeriod.DateOnly;
			var assPrevious = previous.PersonAssignment(true);	
			var visualLayersPrevious = assPrevious.ProjectionService().CreateProjection();	
			var periodPrevious = visualLayersPrevious.Period();	
			var assCurrent = current.PersonAssignment(true);
			var visualLayersCurrent = assCurrent.ProjectionService().CreateProjection();
			var periodCurrent = visualLayersCurrent.Period();

			if (periodPrevious != null)
				earliestDate = new DateOnly(periodPrevious.Value.StartDateTimeLocal(TimeZoneGuard.Instance.TimeZone));

			if (periodCurrent == null) return earliestDate;

			var currentDate = new DateOnly(periodCurrent.Value.StartDateTimeLocal(TimeZoneGuard.Instance.TimeZone));

			if (currentDate < earliestDate)
				earliestDate = currentDate;

			return earliestDate;
		}

		public bool IsNightShift(IScheduleDay scheduleDay)
		{
			var tz = TeleoptiPrincipal.CurrentPrincipal.Regional.TimeZone;
			var personAssignmentPeriod = scheduleDay.PersonAssignment().Period;
			var viewerStartDate = new DateOnly(personAssignmentPeriod.StartDateTimeLocal(tz));
			var viewerEndDate = new DateOnly(personAssignmentPeriod.EndDateTimeLocal(tz).AddMinutes(-1));

			return viewerStartDate != viewerEndDate;
		}
	}
}