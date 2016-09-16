using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class ResourceCalculateDaysDecider : IResourceCalculateDaysDecider
	{
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceCalculateDaysDecider(ITimeZoneGuard timeZoneGuard)
		{
			_timeZoneGuard = timeZoneGuard;
		}

		public IEnumerable<DateOnly> DecideDates(IScheduleDay currentSchedule, IScheduleDay previousSchedule)
		{
			var earliestDate = earliestShiftStartInUserViewPoint(previousSchedule, currentSchedule);
			var latestDate = latestShiftEndInUserViewPoint(previousSchedule, currentSchedule);

			while (earliestDate <= latestDate)
			{
				yield return earliestDate;
				earliestDate = earliestDate.AddDays(1);
			}
		}

		private DateOnly latestShiftEndInUserViewPoint(IScheduleDay previous, IScheduleDay current)
		{
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			var latestDate = previous.DateOnlyAsPeriod.DateOnly;
			var assPrevious = previous.PersonAssignment(true);
			var visualLayersPrevious = assPrevious.ProjectionService().CreateProjection();
			var periodPrevious = visualLayersPrevious.Period();
			var assCurrent = current.PersonAssignment(true);
			var visualLayersCurrent = assCurrent.ProjectionService().CreateProjection();
			var periodCurrent = visualLayersCurrent.Period();

			if (periodPrevious != null)
				latestDate = new DateOnly(periodPrevious.Value.EndDateTimeLocal(currentTimeZone));

			if (periodCurrent == null) return latestDate;

			var currentDate = new DateOnly(periodCurrent.Value.EndDateTimeLocal(currentTimeZone));

			if (currentDate > latestDate)
				latestDate = currentDate;

			return latestDate;
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