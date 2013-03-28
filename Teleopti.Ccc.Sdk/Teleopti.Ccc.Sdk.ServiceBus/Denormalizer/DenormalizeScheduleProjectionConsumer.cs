using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class DenormalizeScheduleProjectionConsumer : ConsumerOf<ProjectionChangedEvent>, ConsumerOf<ProjectionChangedEventForScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;

		public DenormalizeScheduleProjectionConsumer(IUnitOfWorkFactory unitOfWorkFactory, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(ProjectionChangedEvent message)
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

		public void Consume(ProjectionChangedEventForScheduleProjection message)
		{
			createReadModel(message);
		}
	}
}
