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
		public void Handle(ProjectionChangedEvent message)
		{
			createReadModel(message);
		}

		private void createReadModel(ProjectionChangedEventBase message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!message.IsDefaultScenario) return;

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
				unitOfWork.PersistAll();
			}
		}

		public void Handle(ProjectionChangedEventForScheduleProjection message)
		{
			createReadModel(message);
		}
	}
}
