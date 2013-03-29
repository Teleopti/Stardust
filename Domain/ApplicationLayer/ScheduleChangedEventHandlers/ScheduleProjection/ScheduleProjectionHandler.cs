using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection
{
	public class ScheduleProjectionHandler : IHandleEvent<ProjectionChangedEvent>, IHandleEvent<ProjectionChangedEventForScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public ScheduleProjectionHandler(IUnitOfWorkFactory unitOfWorkFactory, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Handle(ProjectionChangedEvent @event)
		{
			createReadModel(@event);
		}

		private void createReadModel(ProjectionChangedEventBase @event)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
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
				unitOfWork.PersistAll();
			}
		}

		public void Handle(ProjectionChangedEventForScheduleProjection @event)
		{
			createReadModel(@event);
		}
	}
}
