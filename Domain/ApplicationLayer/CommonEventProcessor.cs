using System;
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
		public virtual void Process(string tenant, IEvent @event, Type handlerType)
		{
			Process(@event, handlerType);
		}

		public virtual void Process(IEvent @event, Type handlerType)
		{
			using (_initiatorIdentifierScope.OnThisThreadUse(InitiatorIdentifier.FromMessage(@event)))
			{
				try
				{
					using (var scope = _resolve.NewScope())
					{
						dynamic handler = scope.Resolve(handlerType);
						handler.Handle((dynamic)@event);
					}
				}
				catch (Exception)
				{
					sendTrackingMessage(@event);
					throw;
				}
			}
		}

		private void sendTrackingMessage(IEvent @event)
		{
			var commandIdentifier = @event as ICommandIdentifier;
			if (commandIdentifier != null && commandIdentifier.CommandId != Guid.Empty)
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