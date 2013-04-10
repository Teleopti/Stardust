using Rhino.ServiceBus;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class EtlScheduledChangedConsumer // : ConsumerOf<DenormalizedSchedule>
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IEtlReadModelRepository _etlReadModelRepository;

		public EtlScheduledChangedConsumer(ICurrentUnitOfWorkFactory unitOfWorkFactory,IEtlReadModelRepository etlReadModelRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_etlReadModelRepository = etlReadModelRepository;
		}

		public void Consume(DenormalizedSchedule message)
		{
			using (var uow = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
			{
				foreach (var denormalizedScheduleDay in message.ScheduleDays)
				{
					var date = new DateOnly(denormalizedScheduleDay.Date);		
					_etlReadModelRepository.InsertScheduledChanged(new ScheduleChangedReadModel
						{
							DateTime = date,
							PersonId = message.PersonId,
							ScenarioId = message.ScenarioId
						});
				}
				uow.PersistAll();
			}
		}
	}
}