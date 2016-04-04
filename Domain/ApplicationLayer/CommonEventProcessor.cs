using System;
using System.Reflection;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.Domain.MessageBroker;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class CommonEventProcessor
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly IResolve _resolve;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public CommonEventProcessor(
			ResolveEventHandlers resolver,
			IResolve resolve,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolver = resolver;
			_resolve = resolve;
			_trackingMessageSender = trackingMessageSender;
		}

		[TenantScope]
		public virtual void Process(string tenant, IEvent @event, Type handlerType)
		{
			Process(@event, handlerType);
		}

		public virtual void Process(IEvent @event, Type handlerType)
		{
			try
			{
				using (var scope = _resolve.NewScope())
				{
					var handler = scope.Resolve(handlerType);
					var method = _resolver.HandleMethodFor(handler.GetType(), @event);
					try
					{
						method.Invoke(handler, new[] { @event });
					}
					catch (TargetInvocationException e)
					{
						PreserveStack.ForInnerOf(e);
						throw e;
					}
				}
			}
			catch (Exception)
			{
				var commandIdentifier = @event as ICommandIdentifier;
				if (commandIdentifier == null) throw;
				if (commandIdentifier.CommandId != Guid.Empty)
					_trackingMessageSender.SendTrackingMessage(
						@event,
						new TrackingMessage
						{
							Status = TrackingMessageStatus.Failed,
							TrackId = commandIdentifier.CommandId
						});
				throw;
			}
		}

	}
}