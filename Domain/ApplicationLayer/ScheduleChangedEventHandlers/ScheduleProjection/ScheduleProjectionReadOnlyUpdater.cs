using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
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
			var closestLayerToNow = new ProjectionChangedEventLayer
				{
					StartDateTime = DateTime.MaxValue,
					EndDateTime = DateTime.MaxValue
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
					if (isLayerRightNow(layer) || 
						isCurrentLayerCloser(layer, closestLayerToNow))
						closestLayerToNow = layer;
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);
				}
			}
			handleEnqueueRtaMessage(@event, closestLayerToNow);
		}

		private static bool isLayerRightNow(ProjectionChangedEventLayer layer)
		{
			return layer.StartDateTime.ToUniversalTime() <= DateTime.UtcNow &&
			        layer.EndDateTime.ToUniversalTime() >= DateTime.UtcNow;
		}
		
		private static bool isCurrentLayerCloser(ProjectionChangedEventLayer layer, ProjectionChangedEventLayer closestLayerToNow)
		{
			return layer.StartDateTime.ToUniversalTime() >= DateTime.UtcNow &&
			         layer.StartDateTime.ToUniversalTime() < closestLayerToNow.StartDateTime.ToUniversalTime();
		}

		private void handleEnqueueRtaMessage(ProjectionChangedEventBase @event, ProjectionChangedEventLayer closestLayer)
		{
			var nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
			                                                                                           @event.PersonId);
			var layerPeriod = new DateTimePeriod(closestLayer.StartDateTime.ToUniversalTime(),
			                                     closestLayer.EndDateTime.ToUniversalTime());
			if ((nextActivityStartTime != null &&
				 NotifyRtaDecider.ShouldSendMessage(layerPeriod, nextActivityStartTime.Value)) ||
			    (nextActivityStartTime == null &&
				layerPeriod.EndDateTime > DateTime.UtcNow &&
				layerPeriod.EndDateTime != DateTime.MaxValue.ToUniversalTime()))
				_serviceBus.Publish(new ScheduleProjectionReadOnlyChanged
					{
						Datasource = @event.Datasource,
						BusinessUnitId = @event.BusinessUnitId,
						PersonId = @event.PersonId,
						ActivityStartDateTime = layerPeriod.StartDateTime,
						ActivityEndDateTime = layerPeriod.EndDateTime,
						Timestamp = DateTime.UtcNow
					});
		}
	}
}
