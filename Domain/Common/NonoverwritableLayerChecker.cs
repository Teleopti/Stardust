using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Common
{
	public class NonoverwritableLayerChecker: INonoverwritableLayerChecker
	{
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _currentScenario;
		private readonly IUserTimeZone _timeZone;

		public NonoverwritableLayerChecker(IScheduleStorage scheduleStorage, ICurrentScenario currentScenario, IUserTimeZone timeZone)
		{
			_scheduleStorage = scheduleStorage;
			_currentScenario = currentScenario;
			_timeZone = timeZone;
		}

		public IList<OverlappedLayer> GetOverlappedLayersWhenAddingActivity(IPerson person, DateOnly belongsToDate, IActivity activity, DateTimePeriod periodInUtc)
		{
			var scenario = _currentScenario.Current();
			var schedulePeriod = belongsToDate.ToDateOnlyPeriod().Inflate(1);
			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
					schedulePeriod,
					scenario);

			var scheduleDay = schedules[person].ScheduledDay(belongsToDate);
			var overlapLayers = getOverlappedLayersForScheduleDayWhenAdding(activity, periodInUtc, scheduleDay);

			return overlapLayers;
		}

		public IList<OverlappedLayer> GetOverlappedLayersWhenMovingActivity(IPerson person, DateOnly belongsToDate,
			Guid[] layerIdsToMove, DateTime newStartTime)
		{
			var scenario = _currentScenario.Current();
			var schedulePeriod = belongsToDate.ToDateOnlyPeriod().Inflate(1);
			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
					schedulePeriod,
					scenario);

			var scheduleDay = schedules[person].ScheduledDay(belongsToDate);

			var overlapLayers = GetOverlappedLayersForScheduleDayWhenMoving(scheduleDay, layerIdsToMove, newStartTime);

			return overlapLayers;
		}

		public IList<OverlappedLayer> GetOverlappedLayersForScheduleDayWhenMoving(IScheduleDay scheduleDay, Guid[] layerIdsToMove, DateTime newStartTimeInUtc)
		{
			var overlapLayers = new List<OverlappedLayer>();
			var personAssignment = scheduleDay.PersonAssignment().EntityClone();

			if (!layerIdsToMove.Any()) return overlapLayers;

			var targetLayer =
				personAssignment.ShiftLayers.FirstOrDefault(layer => layer.Id == layerIdsToMove.First());

			if (targetLayer == null) return overlapLayers;

			personAssignment.MoveActivityAndKeepOriginalPriority(targetLayer,
				newStartTimeInUtc, null, true);

			Func<IVisualLayer, bool> stickyLayerPredicate = layer =>
			{
				return layer.Payload is IActivity activityLayer &&
					   !(activityLayer.Id == targetLayer.Payload.Id && targetLayer.Period.Contains(layer.Period))
					   && !activityLayer.AllowOverwrite;
			};

			overlapLayers.AddRange(getOverlappedLayersInProjection(scheduleDay, personAssignment, stickyLayerPredicate));

			return overlapLayers;
		}
		private IList<OverlappedLayer> getOverlappedLayersForScheduleDayWhenAdding(IActivity activity, DateTimePeriod period, IScheduleDay scheduleDay)
		{
			var overlappedLayers = new List<OverlappedLayer>();

			var personAssignment = scheduleDay.PersonAssignment();
			if (personAssignment == null || !personAssignment.ShiftLayers.Any()) return overlappedLayers;

			var assForChecking = tryAdding(personAssignment, activity, period);

			Func<IVisualLayer, bool> predictFunc = layer =>
			{
				return layer.Payload is IActivity activityLayer && !activityLayer.AllowOverwrite;
			};

			overlappedLayers.AddRange(getOverlappedLayersInProjection(scheduleDay, assForChecking, predictFunc));
			return overlappedLayers;
		}
		
		IPersonAssignment tryAdding(IPersonAssignment ass, IActivity activity, DateTimePeriod periodInUtc)
		{
			var assForChecking = ass.EntityClone();

			assForChecking.AddActivity(activity, periodInUtc, true);

			return assForChecking;
		}

		private IList<OverlappedLayer> getOverlappedLayersInProjection(IScheduleDay originalScheduleDay,
			IPersonAssignment assForChecking,
			Func<IVisualLayer, bool> stickyLayerPredicate)
		{
			var stickyLayersInNewProjection =
				assForChecking.ProjectionService().CreateProjection().Where(stickyLayerPredicate).ToList();

			var stickyLayersInOldProjection =
				originalScheduleDay.ProjectionService().CreateProjection().Where(stickyLayerPredicate).ToList();

			return
				stickyLayersInOldProjection.Where(
					layer =>
					{
						return !stickyLayersInNewProjection.Any(l => l.Payload.Id == layer.Payload.Id && l.Period.Contains(layer.Period));
					})
					.Select(layer => new OverlappedLayer
					{
						Name = ((IActivity)layer.Payload).Name,
						StartTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.StartDateTime, _timeZone.TimeZone()),
						EndTime = TimeZoneInfo.ConvertTimeFromUtc(layer.Period.EndDateTime, _timeZone.TimeZone())
					}).ToList();
		}
	}

	public interface INonoverwritableLayerChecker
	{
		IList<OverlappedLayer> GetOverlappedLayersWhenAddingActivity(IPerson person, DateOnly belongsToDate, IActivity activity,
			DateTimePeriod periodInUtc);

		IList<OverlappedLayer> GetOverlappedLayersWhenMovingActivity(IPerson person, DateOnly belongsToDate,
			Guid[] layerIdsToMove, DateTime newStartTimeInUtc);

		IList<OverlappedLayer> GetOverlappedLayersForScheduleDayWhenMoving(IScheduleDay scheduleDay, Guid[] layerIdsToMove,
			DateTime newStartTimeInUtc);
	}

	public class OverlappedLayer
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

}
