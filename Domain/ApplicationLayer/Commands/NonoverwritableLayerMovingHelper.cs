using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface INonoverwritableLayerMovingHelper
	{
		TimeSpan GetMovingDistance(IScheduleDay scheduleDay, DateTimePeriod avoidPeriod, Guid shiftLayerId);
		TimeSpan GetMovingDistance(IPerson person, DateOnly date, DateTimePeriod avoidPeriod, Guid shiftLayerId);
		bool IsDestinationValidForMovedShiftLayer(IScheduleDay scheduleDay, ShiftLayer layer,TimeSpan distance);
	}

	public class NonoverwritableLayerMovingHelper : INonoverwritableLayerMovingHelper
	{
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;
		private readonly IScheduleDayProvider _scheduleDayProvider;

		public NonoverwritableLayerMovingHelper(INonoverwritableLayerChecker nonoverwritableLayerChecker, IScheduleDayProvider scheduleDayProvider)
		{
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
			_scheduleDayProvider = scheduleDayProvider;
		}

		public TimeSpan GetMovingDistance(IScheduleDay scheduleDay, DateTimePeriod avoidPeriod, Guid shiftLayerId)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(l => l.Id == shiftLayerId);
			if (shiftLayer == null) return TimeSpan.Zero;

			var toBeforeDistance = getDistanceToMoveToBefore(avoidPeriod, shiftLayer.Period);
			var toAfterDistance = getDistanceToMoveToAfter(avoidPeriod, shiftLayer.Period);

			if ((toBeforeDistance <= TimeSpan.Zero) == (toAfterDistance <= TimeSpan.Zero)) return TimeSpan.Zero;

			var isBeforeLocationValid = IsDestinationValidForMovedShiftLayer(scheduleDay, shiftLayer, toBeforeDistance);
			var isAfterLocationValid = IsDestinationValidForMovedShiftLayer(scheduleDay, shiftLayer, toAfterDistance);
			
			if (isBeforeLocationValid && (!isAfterLocationValid || toBeforeDistance.Duration() <= toAfterDistance.Duration()))
			{
				return toBeforeDistance;
			}

			if (isAfterLocationValid && (!isBeforeLocationValid || toBeforeDistance.Duration() > toAfterDistance.Duration()))
			{
				return toAfterDistance;
			}

			return TimeSpan.Zero;
		}

		public TimeSpan GetMovingDistance(IPerson person, DateOnly date, DateTimePeriod avoidPeriod, Guid shiftLayerId)
		{
			var scheduleDay = _scheduleDayProvider.GetScheduleDay(date, person);
			return GetMovingDistance(scheduleDay, avoidPeriod, shiftLayerId);
		}

		public bool IsDestinationValidForMovedShiftLayer(IScheduleDay scheduleDay, ShiftLayer layer,TimeSpan distance)
		{			
			var periodAtDestination = layer.Period.MovePeriod(distance);
			var projectionPeriod = scheduleDay.ProjectionService().CreateProjection().Period();
			if(!projectionPeriod.HasValue) return false;
			if(!projectionPeriod.Value.Contains(periodAtDestination)) return false;

			var newStartTimeInUtc = layer.Period.StartDateTime.Add(distance);
			var overlappedLayers = _nonoverwritableLayerChecker.GetOverlappedLayersForScheduleDayWhenMoving(scheduleDay,
				new[] { layer.Id.GetValueOrDefault() },newStartTimeInUtc);
			if(overlappedLayers.Count != 0) return false;

			var personAssignmentClone = scheduleDay.PersonAssignment().EntityClone();
			var layerInClone = personAssignmentClone.ShiftLayers.FirstOrDefault(l => l.Id == layer.Id.GetValueOrDefault());
			personAssignmentClone.MoveActivityAndKeepOriginalPriority(layerInClone,newStartTimeInUtc,null, true);
			return personAssignmentClone.ProjectionService()
				.CreateProjection()
				.Any(pl => pl.Period == periodAtDestination && pl.Payload.Id == layer.Payload.Id);
		}


		private TimeSpan getDistanceToMoveToBefore(DateTimePeriod avoidPeriod, DateTimePeriod originalLayerPeriod)
		{
			return avoidPeriod.StartDateTime - originalLayerPeriod.EndDateTime;
		}

		private TimeSpan getDistanceToMoveToAfter(DateTimePeriod avoidPeriod,DateTimePeriod originalLayerPeriod)
		{
			return avoidPeriod.EndDateTime - originalLayerPeriod.StartDateTime;
		}
	}
}
