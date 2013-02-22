using System;
using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;
using Teleopti.Interfaces.Messages.Rta;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class DenormalizeScheduleProjectionConsumer : ConsumerOf<DenormalizedSchedule>, ConsumerOf<DenormalizedScheduleForScheduleProjection>
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IScheduleProjectionReadOnlyRepository _scheduleProjectionReadOnlyRepository;
	    private readonly IServiceBus _serviceBus;

	    public DenormalizeScheduleProjectionConsumer(IUnitOfWorkFactory unitOfWorkFactory, IScheduleProjectionReadOnlyRepository scheduleProjectionReadOnlyRepository, IServiceBus serviceBus)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_scheduleProjectionReadOnlyRepository = scheduleProjectionReadOnlyRepository;
		    _serviceBus = serviceBus;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Consume(DenormalizedSchedule message)
		{
			createReadModel(message);
		}

		private void createReadModel(DenormalizedScheduleBase message)
		{
			using (var unitOfWork = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
			{
				if (!message.IsDefaultScenario) return;

				var date = new DateOnly(message.Date);
				if (!message.IsInitialLoad)
				{
					_scheduleProjectionReadOnlyRepository.ClearPeriodForPerson(
						new DateOnlyPeriod(date, date), message.ScenarioId, message.PersonId);
				}

				foreach (var layer in message.Layers)
				{
					_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, message.ScenarioId, message.PersonId, layer);
				}

				unitOfWork.PersistAll();

                _serviceBus.Send(new UpdatedScheduleDay
                {
                    Datasource = message.Datasource,
                    BusinessUnitId = message.BusinessUnitId,
                    PersonId = message.PersonId,
                    ActivityStartDateTime = message.StartDateTime.GetValueOrDefault(),
                    ActivityEndDateTime = message.EndDateTime.GetValueOrDefault(),
                    Timestamp = DateTime.UtcNow
                });
			}
		}

		public void Consume(DenormalizedScheduleForScheduleProjection message)
		{
			createReadModel(message);
		}
	}
}
