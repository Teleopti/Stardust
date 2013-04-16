using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.Rta;

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
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;
			var nearestLayerToNow = new ProjectionChangedEventLayer();
			nearestLayerToNow.StartDateTime = DateTime.MaxValue; 
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
						if (date == DateTime.UtcNow.Date &&
						    ((layer.StartDateTime.ToUniversalTime() < DateTime.UtcNow &&
						      layer.EndDateTime.ToUniversalTime() > DateTime.UtcNow)
						     ||
						     (layer.StartDateTime.ToUniversalTime() > DateTime.UtcNow &&
						      layer.StartDateTime.ToUniversalTime() < nearestLayerToNow.StartDateTime.ToUniversalTime())))
							nearestLayerToNow = layer;
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);

				}
			}
			if (nearestLayerToNow.StartDateTime != DateTime.MaxValue)
				_serviceBus.Publish(new UpdatedScheduleDay
					{
						Datasource = @event.Datasource,
						BusinessUnitId = @event.BusinessUnitId,
						PersonId = @event.PersonId,
						ActivityStartDateTime = nearestLayerToNow.StartDateTime,
						ActivityEndDateTime = nearestLayerToNow.EndDateTime,
						Timestamp = DateTime.UtcNow
					});
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			createReadModel(@event);
		}
	}
}
