using System;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Sdk.ServiceBus.ApplicationLayer
{
	public class EventsConsumer : 
		ConsumerOf<IEvent>,
		ConsumerOf<EventsPackageMessage>
	{
		private readonly IEventPublisher _publisher;
		private readonly IServiceBus _bus;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(EventsConsumer));

		public EventsConsumer(IEventPublisher publisher, IServiceBus bus, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_publisher = publisher;
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Consume(IEvent message)
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Consuming message of type " + message.GetType().Name);

			var raptorDomainMessage = message as IRaptorDomainMessageInfo;
			if (raptorDomainMessage == null)
			{
				using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
				{
					_publisher.Publish(message);
					unitOfWork.PersistAll();
				}
			}
			else
			{
				using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(raptorDomainMessage)))
				{
					_publisher.Publish(message);
					unitOfWork.PersistAll();
				}
			}
		}

		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events.ToArray());
		}
	}
}