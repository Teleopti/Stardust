using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyUpdater : IHandleEvent<ScheduledResourcesChangedEvent>
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
	    private readonly IPublishEventsFromEventHandlers _serviceBus;

		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IPublishEventsFromEventHandlers serviceBus)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		    _serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduledResourcesChangedEvent @event)
		{
			if (!@event.IsDefaultScenario) return;
			var nearestLayerToNow = new ProjectionChangedEventLayer
				{
					StartDateTime = DateTime.MaxValue
				};

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!@event.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(
						new DateOnlyPeriod(date, date), @event.ScenarioId, @event.PersonId);
				}

				foreach (var layer in scheduleDay.Layers)
				{
					if (isCurrentLayerCloser(date, layer, nearestLayerToNow))
						nearestLayerToNow = layer;
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);
				}
			}
			
			if (@event.ScheduleDays.All(x => x.Date != DateTime.Today))
				return;

			if (!haveChanged(nearestLayerToNow))
				layersHaveBeenRemoved(nearestLayerToNow);

			_serviceBus.Publish(new ScheduleProjectionReadOnlyChanged
				{
					Datasource = @event.Datasource,
					BusinessUnitId = @event.BusinessUnitId,
					PersonId = @event.PersonId,
					ActivityStartDateTime = nearestLayerToNow.StartDateTime,
					ActivityEndDateTime = nearestLayerToNow.EndDateTime,
					Timestamp = DateTime.UtcNow
				});
		}

		private static void layersHaveBeenRemoved(ProjectionChangedEventLayer nearestLayerToNow)
		{
			nearestLayerToNow.StartDateTime = DateTime.UtcNow;
			nearestLayerToNow.EndDateTime = DateTime.UtcNow.AddDays(1);
		}

		private static bool haveChanged(ProjectionChangedEventLayer nearestLayerToNow)
		{
			return nearestLayerToNow.StartDateTime != DateTime.MaxValue;
		}

		private static bool isCurrentLayerCloser(DateOnly date, ProjectionChangedEventLayer layer, ProjectionChangedEventLayer nearestLayerToNow)
		{
			return date == DateOnly.Today &&
			       ((layer.StartDateTime < DateTime.UtcNow &&
			         layer.EndDateTime > DateTime.UtcNow)
			        ||
			        (layer.StartDateTime > DateTime.UtcNow &&
			         layer.StartDateTime < nearestLayerToNow.StartDateTime));
		}
	}
}
