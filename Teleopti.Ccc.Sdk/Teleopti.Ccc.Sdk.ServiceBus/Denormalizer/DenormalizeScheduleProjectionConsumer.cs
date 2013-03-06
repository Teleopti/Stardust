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
			var nearestLayerToNow = new DenormalizedScheduleProjectionLayer();
			nearestLayerToNow.StartDateTime = DateTime.MaxValue;
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
						if (date == DateTime.UtcNow.Date &&
						    ((layer.StartDateTime.ToUniversalTime() < DateTime.UtcNow &&
						      layer.EndDateTime.ToUniversalTime() > DateTime.UtcNow)
						     ||
						     (layer.StartDateTime.ToUniversalTime() > DateTime.UtcNow &&
						      layer.StartDateTime.ToUniversalTime() < nearestLayerToNow.StartDateTime.ToUniversalTime())))
							nearestLayerToNow = layer;

						_scheduleProjectionReadOnlyRepository.AddProjectedLayer(date, message.ScenarioId, message.PersonId, layer);
					}
				}
				unitOfWork.PersistAll();
			}
			if (nearestLayerToNow.StartDateTime != DateTime.MaxValue)
				_serviceBus.Send(new UpdatedScheduleDay
					{
						Datasource = message.Datasource,
						BusinessUnitId = message.BusinessUnitId,
						PersonId = message.PersonId,
						ActivityStartDateTime = nearestLayerToNow.StartDateTime,
						ActivityEndDateTime = nearestLayerToNow.EndDateTime,
						Timestamp = DateTime.UtcNow
					});
		}

		public void Consume(DenormalizedScheduleForScheduleProjection message)
		{
			createReadModel(message);
		}
	}
}
