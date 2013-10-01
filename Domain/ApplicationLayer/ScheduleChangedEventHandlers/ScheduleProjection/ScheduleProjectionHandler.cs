using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationRtaQueue;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionHandler : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForScheduleProjection>
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
	    private readonly IPublishEventsFromEventHandlers _serviceBus;

		public ScheduleProjectionHandler(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IPublishEventsFromEventHandlers serviceBus)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		    _serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			var closestLayerToNow = createReadModel(@event);
			handleEnqueueRtaMessage(@event, closestLayerToNow);
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			var closestLayerToNow = createReadModel(@event);
			handleEnqueueRtaMessage(@event, closestLayerToNow);
		}

		private DateTimePeriod? createReadModel(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) 
				return null;
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

			return new DateTimePeriod(closestLayerToNow.StartDateTime.ToUniversalTime(), closestLayerToNow.EndDateTime.ToUniversalTime());
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

		private void handleEnqueueRtaMessage(ProjectionChangedEventBase @event, DateTimePeriod? closestLayer)
		{
			if (closestLayer == null) return;

			var nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
			                                                                                           @event.PersonId);
			if ((nextActivityStartTime != null &&
			     NotifyRtaDecider.ShouldSendMessage(closestLayer.Value, nextActivityStartTime.Value)) ||
			    (nextActivityStartTime == null &&
				closestLayer.Value.EndDateTime > DateTime.UtcNow &&
				closestLayer.Value.EndDateTime != DateTime.MaxValue.ToUniversalTime()))
				_serviceBus.Publish(new UpdatedScheduleDay
					{
						Datasource = @event.Datasource,
						BusinessUnitId = @event.BusinessUnitId,
						PersonId = @event.PersonId,
						ActivityStartDateTime = closestLayer.Value.StartDateTime,
						ActivityEndDateTime = closestLayer.Value.EndDateTime,
						Timestamp = DateTime.UtcNow
					});
		}
	}
}
