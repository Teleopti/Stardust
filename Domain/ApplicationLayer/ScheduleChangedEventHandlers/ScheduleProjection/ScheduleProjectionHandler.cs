using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionHandler : IHandleEvent<ProjectionChangedEvent>, IHandleEvent<ProjectionChangedEventForScheduleProjection>
	{
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public ScheduleProjectionHandler(IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase @event)
		{
			if (!@event.IsDefaultScenario) return;

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
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, @event.ScenarioId, @event.PersonId, layer);
				}
			}
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			createReadModel(@event);
		}
	}
}
