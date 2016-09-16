using System.Collections.Generic;
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
			var previousLayers = previousSchedule.PersonAssignment(true).ProjectionService().CreateProjection();
			var currentLayers = currentSchedule.PersonAssignment(true).ProjectionService().CreateProjection();
			var earliestDate = earliestShiftStartInUserViewPoint(previousLayers, currentLayers, currentSchedule.DateOnlyAsPeriod.DateOnly);
			var latestDate = latestShiftEndInUserViewPoint(previousLayers, currentLayers, previousSchedule.DateOnlyAsPeriod.DateOnly);

			while (earliestDate <= latestDate)
			{
				yield return earliestDate;
				earliestDate = earliestDate.AddDays(1);
			}
		}

		private DateOnly latestShiftEndInUserViewPoint(IVisualLayerCollection visualLayersPrevious, IVisualLayerCollection visualLayersCurrent, DateOnly date)
		{
			var latestDate = date;
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			var periodPrevious = visualLayersPrevious.Period();
			var periodCurrent = visualLayersCurrent.Period();

			if (periodPrevious != null)
				latestDate = new DateOnly(periodPrevious.Value.EndDateTimeLocal(currentTimeZone));

			if (periodCurrent == null) return latestDate;

			var currentDate = new DateOnly(periodCurrent.Value.EndDateTimeLocal(currentTimeZone));

			if (currentDate > latestDate)
				latestDate = currentDate;

			return latestDate;
		}

		private DateOnly earliestShiftStartInUserViewPoint(IVisualLayerCollection visualLayersPrevious, IVisualLayerCollection visualLayersCurrent, DateOnly date)
		{
			var earliestDate = date;
			var currentTimeZone = _timeZoneGuard.CurrentTimeZone();
			var periodPrevious = visualLayersPrevious.Period();
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