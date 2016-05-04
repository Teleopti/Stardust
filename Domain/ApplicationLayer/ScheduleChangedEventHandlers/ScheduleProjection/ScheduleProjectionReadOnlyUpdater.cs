using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
#pragma warning disable 618
	public class ScheduleProjectionReadOnlyUpdater : 
		IHandleEvent<ProjectionChangedEvent>, 
		IHandleEvent<ProjectionChangedEventForScheduleProjection>,
		IRunOnServiceBus
#pragma warning restore 618
	{
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;
	    
		public ScheduleProjectionReadOnlyUpdater(IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}
		
		public void Handle(ProjectionChangedEvent @event)
		{
			handleProjectionChanged(@event);
		}

		private void handleProjectionChanged(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;
			foreach (var scheduleDay in @event.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!@event.IsInitialLoad)
					_scheduleProjectionReadOnlyPersister.ClearDayForPerson(date, @event.ScenarioId, @event.PersonId,
						@event.ScheduleLoadTimestamp);

				if (scheduleDay.Shift == null) continue;
				foreach (var layer in scheduleDay.Shift.Layers)
				{
					_scheduleProjectionReadOnlyPersister.AddProjectedLayer(
						new ScheduleProjectionReadOnlyModel
						{
							PersonId = @event.PersonId,
							ScenarioId = @event.ScenarioId,
							BelongsToDate = date,
							PayloadId = layer.PayloadId,
							WorkTime = layer.WorkTime,
							ContractTime = layer.ContractTime,
							StartDateTime = layer.StartDateTime,
							EndDateTime = layer.EndDateTime,
							Name = layer.Name,
							ShortName = layer.ShortName,
							DisplayColor = layer.DisplayColor,
							ScheduleLoadedTime = @event.ScheduleLoadTimestamp,
						});
				}
			}
		}
		
		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			handleProjectionChanged(@event);
		}
	}
}
