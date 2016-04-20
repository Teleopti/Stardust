﻿using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionReadOnlyUpdater : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
#pragma warning disable 618
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
	    private readonly IEventPublisher _serviceBus;
		private readonly INow _now;

		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, 
																							IEventPublisher serviceBus,
																							INow now)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		    _serviceBus = serviceBus;
			_now = now;
		}
		
		public void Handle(ProjectionChangedEvent @event)
		{
			handleProjectionChanged(@event);
		}

		private void handleProjectionChanged(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;
			var closestLayerToNow = new ProjectionChangedEventLayer
			{
				StartDateTime = DateTime.MaxValue.Date,
				EndDateTime = DateTime.MaxValue.Date
			};
			var isChanged = false;
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!@event.IsInitialLoad)
				{
					var count =_scheduleProjectionReadOnlyRepository.ClearDayForPerson(
						date, @event.ScenarioId, @event.PersonId, @event.ScheduleLoadTimestamp);
					if (count > 0)
						isChanged = true;
				}

				if (scheduleDay.Shift == null) continue;
				foreach (var layer in scheduleDay.Shift.Layers)
				{
					if (isLayerRightNow(layer) ||
						isCurrentLayerCloser(layer, closestLayerToNow))
						closestLayerToNow = layer;
					if(_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer, @event.ScheduleLoadTimestamp) > 0)
						isChanged = true;
				}
			}
			if (isChanged)
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
						LogOnDatasource = @event.LogOnDatasource,
						LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
						PersonId = @event.PersonId,
						ActivityStartDateTime = layerPeriod.StartDateTime,
						ActivityEndDateTime = layerPeriod.EndDateTime,
						Timestamp = now
					});
			}
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			handleProjectionChanged(@event);
		}
	}
}
