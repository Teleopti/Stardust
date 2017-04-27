using System;
using System.Collections.Generic;
using System.Linq;
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
		public virtual void Process(string tenant, IEnumerable<IEvent> events, Type handlerType)
		{
			Process(events, handlerType);
		}

		public virtual void Process(IEvent @event, Type handlerType)
		{
			Process(new[]{@event}, handlerType);
		}

		public virtual void Process(IEnumerable<IEvent> events, Type handlerType)
		{
			using (_initiatorIdentifierScope.OnThisThreadUse(InitiatorIdentifier.FromMessage(events.First())))
			{
				try
				{
					using (var scope = _resolve.NewScope())
					{
						dynamic handler = scope.Resolve(handlerType);
						if (handlerType.GetInterfaces().Contains(typeof(IHandleEvents)))
						{
							handler.Handle((dynamic) events);
						}
						else
						{
							handler.Handle((dynamic) events.Single());
						}
					}
				}
				catch (Exception)
				{
					sendTrackingMessage(events.First());
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