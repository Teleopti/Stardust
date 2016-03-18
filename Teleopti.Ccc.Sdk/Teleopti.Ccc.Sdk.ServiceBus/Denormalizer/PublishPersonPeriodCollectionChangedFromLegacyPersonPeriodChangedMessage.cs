using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class PublishPersonPeriodCollectionChangedFromLegacyPersonPeriodChangedMessage : 
		ConsumerOf<PersonPeriodChangedMessage>
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public PublishPersonPeriodCollectionChangedFromLegacyPersonPeriodChangedMessage(IEventPublisher eventPublisher, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_eventPublisher = eventPublisher;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Consume(PersonPeriodChangedMessage message)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_eventPublisher.Publish(new PersonPeriodCollectionChangedEvent
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
					InitiatorId = message.InitiatorId,
					SerializedPersonPeriod = message.SerializedPersonPeriod,
					Timestamp = message.Timestamp
				});
				uow.PersistAll();
			}
		}
	}
}

