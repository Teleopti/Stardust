
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages.Denormalize;

namespace Teleopti.Ccc.Sdk.ServiceBus.Denormalizer
{
	public class PublishGroupPageCollectionChangedFromLegacyGroupPageChangedMessage : ConsumerOf<GroupPageChangedMessage>
	{
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		public PublishGroupPageCollectionChangedFromLegacyGroupPageChangedMessage(IEventPublisher eventPublisher, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory)
		{
			_eventPublisher = eventPublisher;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
		}

		public void Consume(GroupPageChangedMessage message)
		{
			using (var uow = _currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_eventPublisher.Publish(new GroupPageCollectionChangedEvent
				{
					LogOnBusinessUnitId = message.LogOnBusinessUnitId,
					LogOnDatasource = message.LogOnDatasource,
					InitiatorId = message.InitiatorId,
					SerializedGroupPage = message.SerializedGroupPage,
					Timestamp = message.Timestamp
				});
				uow.PersistAll();
			}
		}
	}
}
