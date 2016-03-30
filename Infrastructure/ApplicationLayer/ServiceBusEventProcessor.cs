using System;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class ServiceBusEventProcessor
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public ServiceBusEventProcessor(
			ResolveEventHandlers resolver,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			ICurrentUnitOfWorkFactory unitOfWorkFactory,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolver = resolver;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_unitOfWorkFactory = unitOfWorkFactory;
			_trackingMessageSender = trackingMessageSender;
		}

		public void Process(IEvent @event)
		{
			var logOnInfo = @event as ILogOnContext;
			var initiatorInfo = @event as IInitiatorContext;
			var commandIdentifier = @event as ICommandIdentifier;

			try
			{
				if (logOnInfo == null)
				{
					process(@event);
				}
				else
				{
					if (initiatorInfo == null)
					{
						using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
						{
							process(@event);
							unitOfWork.PersistAll();
						}
					}
					else
					{
						using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(initiatorInfo)))
						{
							process(@event);
							unitOfWork.PersistAll();
						}
					}

				}

			}
			catch (Exception)
			{
				if (_trackingMessageSender == null) throw;
				if (commandIdentifier == null) throw;
				if (commandIdentifier.CommandId != Guid.Empty)
					_trackingMessageSender.SendTrackingMessage(@event, new TrackingMessage
					{
						Status = TrackingMessageStatus.Failed,
						TrackId = commandIdentifier.CommandId
					});
				throw;
			}
		}

		private void process(IEvent @event)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);

			var handlers = _resolver.ResolveServiceBusHandlersForEvent(@event);

			foreach (var handler in handlers)
			{
				new SyncPublishTo(_resolver, handler).Publish(@event);
			}

		}

	}
}