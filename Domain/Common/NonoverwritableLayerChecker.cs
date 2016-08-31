using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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

		public IList<OverlappedLayer> GetOverlappedLayersWhenAddingActivity(IPerson person, DateOnly belongsToDate, IActivity activity, DateTimePeriod period)
		{
			var scenario = _currentScenario.Current();
			var schedulePeriod = (new DateOnlyPeriod(belongsToDate, belongsToDate)).Inflate(1);
			var schedules = _scheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(person, new ScheduleDictionaryLoadOptions(false, false),
					schedulePeriod,
					scenario);

			var scheduleDay = schedules[person].ScheduledDay(belongsToDate);
			var overlapLayers = getOverlappedLayersForScheduleDayWhenAdding(activity, period, scheduleDay);

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
				var activityLayer = layer.Payload as IActivity;
				return activityLayer != null && !activityLayer.AllowOverwrite;
			};

			overlappedLayers.AddRange(getOverlappedLayersInProjection(scheduleDay, assForChecking, predictFunc));
			return overlappedLayers;
		}

		IPersonAssignment tryAdding(IPersonAssignment ass, IActivity activity, DateTimePeriod period)
		{
			var assForChecking = ass.EntityClone();

			var activityPeriod = new DateTimePeriod(TimeZoneHelper.ConvertToUtc(period.StartDateTime, _timeZone.TimeZone()),
				TimeZoneHelper.ConvertToUtc(period.EndDateTime, _timeZone.TimeZone()));

			assForChecking.AddActivity(activity, activityPeriod);

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
			DateTimePeriod period);
	}

	public class OverlappedLayer
	{
		public string Name { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

}
