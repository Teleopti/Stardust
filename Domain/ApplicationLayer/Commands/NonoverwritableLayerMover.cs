using System;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Commands
{
	public interface INonoverwritableLayerMover
	{
		bool MoveShiftLayer(IScheduleDay scheduleDay, DateTimePeriod avoidPeriod, Guid shiftLayerId);
		bool IsDestinationValidForMovedShiftLayer(IScheduleDay scheduleDay, IShiftLayer layer,TimeSpan distance);
	}

	public class NonoverwritableLayerMover : INonoverwritableLayerMover
	{
		private readonly INonoverwritableLayerChecker _nonoverwritableLayerChecker;

		public NonoverwritableLayerMover(INonoverwritableLayerChecker nonoverwritableLayerChecker)
		{
			_nonoverwritableLayerChecker = nonoverwritableLayerChecker;
		}

		public bool MoveShiftLayer(IScheduleDay scheduleDay, DateTimePeriod avoidPeriod, Guid shiftLayerId)
		{
			var personAssignment = scheduleDay.PersonAssignment();
			var shiftLayer = personAssignment.ShiftLayers.FirstOrDefault(l => l.Id == shiftLayerId);
			if (shiftLayer == null) return false;

			var toBeforeDistance = getDistanceToMoveToBefore(avoidPeriod, shiftLayer.Period);
			var toAfterDistance = getDistanceToMoveToAfter(avoidPeriod, shiftLayer.Period);

			var isBeforeLocationValid = IsDestinationValidForMovedShiftLayer(scheduleDay, shiftLayer, toBeforeDistance);
			var isAfterLocationValid = IsDestinationValidForMovedShiftLayer(scheduleDay, shiftLayer, toAfterDistance);

			if (isBeforeLocationValid && (!isAfterLocationValid || toBeforeDistance <= toAfterDistance))
			{
				moveLayer(personAssignment, shiftLayer, toBeforeDistance);
				return true;
			}

			if (isAfterLocationValid && (!isBeforeLocationValid || toBeforeDistance > toAfterDistance))
			{
				moveLayer(personAssignment, shiftLayer, toAfterDistance);
				return true;
			}

			return false;
		}

		
		public bool IsDestinationValidForMovedShiftLayer(IScheduleDay scheduleDay, IShiftLayer layer,TimeSpan distance)
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
			personAssignmentClone.MoveActivityAndKeepOriginalPriority(layerInClone,newStartTimeInUtc,null);
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

		private void moveLayer(IPersonAssignment pa,IShiftLayer layer,TimeSpan distance)
		{
			pa.MoveActivityAndKeepOriginalPriority(layer,layer.Period.StartDateTime.Add(distance),null);
		}


	}
}
