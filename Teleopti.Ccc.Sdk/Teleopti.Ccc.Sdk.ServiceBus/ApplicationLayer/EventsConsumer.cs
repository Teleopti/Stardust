using System;
using log4net;
using Rhino.ServiceBus;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.MessageBroker.Events;
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
		private readonly ITrackingMessageSender _trackingMessageSender;

		private readonly static ILog Logger = LogManager.GetLogger(typeof(EventsConsumer));

		public EventsConsumer(IEventPublisher publisher, IServiceBus bus, ICurrentUnitOfWorkFactory unitOfWorkFactory, ITrackingMessageSender trackingMessageSender)
		{
			_publisher = publisher;
			_bus = bus;
			_unitOfWorkFactory = unitOfWorkFactory;
			_trackingMessageSender = trackingMessageSender;
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
				try
				{
					using (
						var unitOfWork =
							_unitOfWorkFactory.LoggedOnUnitOfWorkFactory()
								.CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(raptorDomainMessage)))
					{
						_publisher.Publish(message);
						unitOfWork.PersistAll();
					}
				}
				catch (Exception)
				{
					if (_trackingMessageSender != null && message is ITrackableEvent && message is RaptorDomainEvent)
					{
						var msg1 = message as ITrackableEvent;
						var msg2 = message as RaptorDomainEvent;
						if (msg1.TrackId != Guid.Empty)
							_trackingMessageSender.SendTrackingMessage(msg2.InitiatorId, msg2.BusinessUnitId, new TrackingMessage
							{
								Status = TrackingMessageStatus.Failed,
								TrackId = msg1.TrackId
							});
					}
					throw;
				}
				
			}
		}

		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events.ToArray());
		}
	}
}