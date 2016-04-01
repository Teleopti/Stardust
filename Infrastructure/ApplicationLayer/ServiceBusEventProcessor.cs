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
	public class CommonEventProcessor
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public CommonEventProcessor(
			ResolveEventHandlers resolver,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolver = resolver;
			_trackingMessageSender = trackingMessageSender;
		}

		public void Process(IEvent @event, object handler)
		{
			var commandIdentifier = @event as ICommandIdentifier;
			try
			{
				new SyncPublishTo(_resolver, handler).Publish(@event);
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
	}

	public class ServiceBusEventProcessor
	{
		private readonly CommonEventProcessor _processor;
		private readonly ResolveEventHandlers _resolver;
		private readonly IEventInfrastructureInfoPopulator _eventInfrastructureInfoPopulator;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;

		public ServiceBusEventProcessor(
			CommonEventProcessor processor,
			ResolveEventHandlers resolver,
			IEventInfrastructureInfoPopulator eventInfrastructureInfoPopulator,
			ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_processor = processor;
			_resolver = resolver;
			_eventInfrastructureInfoPopulator = eventInfrastructureInfoPopulator;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public void Process(IEvent @event)
		{
			_eventInfrastructureInfoPopulator.PopulateEventContext(@event);

			var initiatorInfo = @event as IInitiatorContext;

			if (initiatorInfo == null)
			{
				using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
				{
					foreach (var handler in _resolver.ResolveServiceBusHandlersForEvent(@event))
						_processor.Process(@event, handler);
					unitOfWork.PersistAll();
				}
			}
			else
			{
				using (var unitOfWork = _unitOfWorkFactory.Current().CreateAndOpenUnitOfWork(new InitiatorIdentifierFromMessage(initiatorInfo)))
				{
					foreach (var handler in _resolver.ResolveServiceBusHandlersForEvent(@event))
						_processor.Process(@event, handler);
					unitOfWork.PersistAll();
				}
			}
		}

	}
}
