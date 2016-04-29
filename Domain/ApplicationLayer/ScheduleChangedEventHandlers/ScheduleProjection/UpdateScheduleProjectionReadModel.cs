using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class UpdateScheduleProjectionReadModel : IUpdateScheduleProjectionReadModel
	{
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private readonly IScheduleProjectionReadOnlyPersister _scheduleProjectionReadOnlyPersister;

		public UpdateScheduleProjectionReadModel(IProjectionChangedEventBuilder projectionChangedEventBuilder, IScheduleProjectionReadOnlyPersister scheduleProjectionReadOnlyPersister)
		{
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
			_scheduleProjectionReadOnlyPersister = scheduleProjectionReadOnlyPersister;
		}

		public void Execute(IScheduleRange scheduleRange, DateOnlyPeriod dateOnlyPeriod)
		{
			_projectionChangedEventBuilder
				.Build<ProjectionChangedEvent>(
					new ScheduleChangedEvent
					{
						ScenarioId = scheduleRange.Scenario.Id.GetValueOrDefault(),
						PersonId = scheduleRange.Person.Id.GetValueOrDefault()
					},
					scheduleRange,
					dateOnlyPeriod)
				.ForEach(updateReadModel)
				;
		}

		private void updateReadModel(ProjectionChangedEvent message)
		{
			foreach (var scheduleDay in message.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!message.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyPersister.ClearDayForPerson(
						date, message.ScenarioId, message.PersonId, message.ScheduleLoadTimestamp);
				}

				if (scheduleDay.Shift == null) continue;
				foreach (var layer in scheduleDay.Shift.Layers)
				{
					_scheduleProjectionReadOnlyPersister.AddProjectedLayer(date, message.ScenarioId, message.PersonId, layer, message.ScheduleLoadTimestamp);
				}
			}
		}
	}
}