using System;
using log4net;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;
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

		public void Consume(IEvent @event)
		{
			if (Logger.IsDebugEnabled)
				Logger.Debug("Consuming message of type " + @event.GetType().Name);

			var logOnInfo = @event as ILogOnInfo;
			var initiatorInfo = @event as IInitiatorInfo;
			var trackInfo = @event as ITrackInfo;

			try
			{
				if (logOnInfo == null)
				{
					_publisher.Publish(@event);
				}
				else
				{
					if (initiatorInfo == null)
					{
						using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
						{
							_publisher.Publish(@event);
							unitOfWork.PersistAll();
						}
					}
					else
					{
						using (var unitOfWork = _unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(initiatorInfo)))
						{
							_publisher.Publish(@event);
							unitOfWork.PersistAll();
						}
					}

				}

			}
			catch (Exception)
			{
				if (_trackingMessageSender == null) throw;
				if (trackInfo == null) throw;
				if (trackInfo.TrackId != Guid.Empty)
					_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
					{
						Status = TrackingMessageStatus.Failed,
						TrackId = trackInfo.TrackId
					});
				throw;
			}

		}

		public void Consume(EventsPackageMessage message)
		{
			_bus.Send(message.Events.ToArray());
		}
	}
}