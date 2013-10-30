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

				foreach (var layer in scheduleDay.Layers)
				{
					if (isLayerRightNow(layer) || 
						isCurrentLayerCloser(layer, closestLayerToNow))
						closestLayerToNow = layer;
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);
				}
			}

			return new DateTimePeriod(DateTime.SpecifyKind(closestLayerToNow.StartDateTime, DateTimeKind.Utc),
			                          DateTime.SpecifyKind(closestLayerToNow.EndDateTime, DateTimeKind.Utc));
		}

		private static bool isLayerRightNow(ProjectionChangedEventLayer layer)
		{
			return layer.StartDateTime <= DateTime.UtcNow &&
			        layer.EndDateTime >= DateTime.UtcNow;
		}
		
		private static bool isCurrentLayerCloser(ProjectionChangedEventLayer layer, ProjectionChangedEventLayer closestLayerToNow)
		{
			return layer.StartDateTime >= DateTime.UtcNow &&
			         layer.StartDateTime < closestLayerToNow.StartDateTime;
		}

		private void handleEnqueueRtaMessage(ProjectionChangedEventBase @event, DateTimePeriod? closestLayer)
		{
			if (closestLayer == null) return;

			var nextActivityStartTime = _scheduleProjectionReadOnlyRepository.GetNextActivityStartTime(DateTime.UtcNow,
			                                                                                           @event.PersonId);
			if (NotifyRtaDecider.ShouldSendMessage(closestLayer.Value, nextActivityStartTime) &&
				@event.ScheduleDays.Any(d => d.Date >= DateTime.UtcNow.Date))
			{
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
}
