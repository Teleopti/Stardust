using System;
using System.Collections.Generic;
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
		private readonly IInitiatorIdentifierScope _initiatorIdentifierScope;
		private readonly ITrackingMessageSender _trackingMessageSender;

		public CommonEventProcessor(
			ResolveEventHandlers resolver,
			IResolve resolve,
			IInitiatorIdentifierScope initiatorIdentifierScope,
			ITrackingMessageSender trackingMessageSender)
		{
			_resolver = resolver;
			_resolve = resolve;
			_initiatorIdentifierScope = initiatorIdentifierScope;
			_trackingMessageSender = trackingMessageSender;
		}

		[TenantScope]
		public virtual void Process(string tenant, IEvent @event, Type handlerType)
		{
			Process(@event, handlerType);
		}

		private Action<T> MakeDelegate<T>(T eventType, object handler, MethodInfo method)
		{
			var actionType = typeof(Action<>).MakeGenericType(eventType.GetType());
			return (Action<T>)Delegate.CreateDelegate(actionType, handler, method);
		}
		
		public virtual void Process(IEvent @event, Type handlerType)
		{
			using (_initiatorIdentifierScope.OnThisThreadUse(InitiatorIdentifier.FromMessage(@event)))
			{
				try
				{
					using (var scope = _resolve.NewScope())
					{
						var handler = scope.Resolve(handlerType);
						var method = _resolver.HandleMethodFor(handler.GetType(), @event);
						//method.Invoke(handler, new[] { @event });

						//var actionType = typeof(Action<>).MakeGenericType(@event.GetType());
						//dynamic action = Delegate.CreateDelegate(actionType, handler, method);
						//action.Invoke(@event);
						//action.Method.Invoke(handler, new [] { @event });

						var d = MakeDelegate((dynamic) @event, handler, method);
						//var actionType = typeof(Action<>).MakeGenericType(((dynamic)@event).GetType());
						//var d = (Action<dynamic>)Delegate.CreateDelegate(actionType, handler, method);
						d.Invoke((dynamic)@event);

					}
				}
				catch (TargetInvocationException e)
				{
					PreserveStack.ForInnerOf(e);
					sendTrackingMessage(@event);
					throw e;
				}
				catch (Exception e)
				{
					PreserveStack.For(e);
					sendTrackingMessage(@event);
					throw e;
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