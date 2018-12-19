using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MessageBroker;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class CommonEventProcessor
	{
		private readonly IResolve _resolve;
		private readonly IInitiatorIdentifierScope _initiatorIdentifierScope;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public CommonEventProcessor(
			IResolve resolve,
			IInitiatorIdentifierScope initiatorIdentifierScope,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolve = resolve;
			_initiatorIdentifierScope = initiatorIdentifierScope;
			_trackingMessageSender = trackingMessageSender;
		}

		[TenantScope]
		public virtual void Process(string tenant, IEvent @event, IEnumerable<IEvent> package, Type handlerType)
		{
			if (@event != null)
				processWithInitiatorAndTrackingMessage(@event, handlerType);
			else
				invokeHandler(package, handlerType);
		}
		
		[Obsolete("Without tenant scope for the job. Dont use.")]
		public virtual void Process(IEvent @event, Type handlerType)
		{
			processWithInitiatorAndTrackingMessage(@event, handlerType);
		}

		private void processWithInitiatorAndTrackingMessage(IEvent @event, Type handlerType)
		{
			using (_initiatorIdentifierScope.OnThisThreadUse(InitiatorIdentifier.FromMessage(@event)))
			{
				try
				{
					invokeHandler(@event, handlerType);
				}
				catch (Exception)
				{
					sendTrackingMessage(@event);
					throw;
				}
			}
		}
		
		private void invokeHandler(object argument, Type handlerType)
		{
			using (var scope = _resolve.NewScope())
			{
				dynamic handler = scope.Resolve(handlerType);
				handler.Handle((dynamic) argument);
			}
		}

		private void sendTrackingMessage(IEvent @event)
		{
			if (@event is ICommandIdentifier commandIdentifier && commandIdentifier.CommandId != Guid.Empty)
			{
				_trackingMessageSender.SendTrackingMessage(
					@event,
					new TrackingMessage
					{
						Status = TrackingMessageStatus.Failed,
						TrackId = commandIdentifier.CommandId
					});
			}
		}
	}
}