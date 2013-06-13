using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class UpdateScheduleProjectionReadModel : IUpdateScheduleProjectionReadModel
	{
		private readonly IProjectionChangedEventBuilder _projectionChangedEventBuilder;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public UpdateScheduleProjectionReadModel(IProjectionChangedEventBuilder projectionChangedEventBuilder, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_projectionChangedEventBuilder = projectionChangedEventBuilder;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods",
			MessageId = "0")]
		public void Execute(IScheduleRange scheduleRange, DateOnlyPeriod dateOnlyPeriod)
		{
			_projectionChangedEventBuilder.Build<ProjectionChangedEvent>(
				new ScheduleChangedEvent
					{
						ScenarioId = scheduleRange.Scenario.Id.GetValueOrDefault(),
						PersonId = scheduleRange.Person.Id.GetValueOrDefault()
					}, scheduleRange, dateOnlyPeriod, updateReadModel);
		}

		private void updateReadModel(ProjectionChangedEvent message)
		{
			foreach (var scheduleDay in message.ScheduleDays)
			{
				var date = new DateOnly(scheduleDay.Date);
				if (!message.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(
						new DateOnlyPeriod(date, date), message.ScenarioId, message.PersonId);
				}

				foreach (var layer in scheduleDay.Layers)
				{
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, message.ScenarioId, message.PersonId, layer);
				}
			}
		}
	}
}