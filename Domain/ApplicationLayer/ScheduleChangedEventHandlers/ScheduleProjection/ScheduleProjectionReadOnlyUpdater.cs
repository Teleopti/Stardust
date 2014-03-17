using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyUpdater : IHandleEvent<ScheduledResourcesChangedEvent>
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
	    private readonly IPublishEventsFromEventHandlers _serviceBus;
		private readonly INow _now;

		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, 
																							IPublishEventsFromEventHandlers serviceBus,
																							INow now)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		    _serviceBus = serviceBus;
			_now = now;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ScheduledResourcesChangedEvent @event)
		{
			if (!@event.IsDefaultScenario) return;
			var closestLayerToNow = new ProjectionChangedEventLayer
				{
					StartDateTime = DateTime.MaxValue.Date,
					EndDateTime = DateTime.MaxValue.Date
				};

			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!@event.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(
						new DateOnlyPeriod(date, date), @event.ScenarioId, @event.PersonId);
				}

				if (scheduleDay.Shift == null) continue;
				foreach (var layer in scheduleDay.Shift.Layers)
				{
					if (isLayerRightNow(layer) ||
					    isCurrentLayerCloser(layer, closestLayerToNow))
						closestLayerToNow = layer;
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);
				}
			}
			handleEnqueueRtaMessage(@event, closestLayerToNow);
		}

		private bool isLayerRightNow(ProjectionChangedEventLayer layer)
		{
			var now = _now.UtcDateTime();
			return layer.StartDateTime <= now &&
			        layer.EndDateTime >= now;
		}
		
		private bool isCurrentLayerCloser(ProjectionChangedEventLayer layer, ProjectionChangedEventLayer closestLayerToNow)
		{
			return layer.StartDateTime >= _now.UtcDateTime() &&
			         layer.StartDateTime < closestLayerToNow.StartDateTime;
		}

		private void handleEnqueueRtaMessage(ProjectionChangedEventBase @event, ProjectionChangedEventLayer closestLayer)
		{
			var now = _now.UtcDateTime();
			var nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(now,
			                                                                                           @event.PersonId);
			var layerPeriod = new DateTimePeriod(DateTime.SpecifyKind(closestLayer.StartDateTime, DateTimeKind.Utc),
				DateTime.SpecifyKind(closestLayer.EndDateTime, DateTimeKind.Utc));
			if (NotifyRtaDecider.ShouldSendMessage(layerPeriod, nextActivityStartTime) &&
				@event.ScheduleDays.Any(d => d.Date >= now.Date))
			{
				_serviceBus.Publish(new ScheduleProjectionReadOnlyChanged
					{
						Datasource = @event.Datasource,
						BusinessUnitId = @event.BusinessUnitId,
						PersonId = @event.PersonId,
						ActivityStartDateTime = layerPeriod.StartDateTime,
						ActivityEndDateTime = layerPeriod.EndDateTime,
						Timestamp = now
					});
			}
		}
	}
}
