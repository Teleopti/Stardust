using System;
using System.Collections;
using System.Collections.Generic;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IResolve
	{
		object Resolve(Type type);
	}

	public class AutofacResolve : IResolve
	{
		private readonly IComponentContext _componentContext;

		public AutofacResolve(IComponentContext componentContext) {
			_componentContext = componentContext;
		}

		public object Resolve(Type type)
		{
			return _componentContext.Resolve(type);
		}
	}

	public class EventPublisher : IEventPublisher
	{
		private readonly IResolve _resolver;

		public EventPublisher(IResolve resolver)
		{
			_resolver = resolver;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Publish(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof (IEnumerable<>).MakeGenericType(handlerType);
			var handlers = _resolver.Resolve(enumerableHandlerType) as IEnumerable;
			foreach (var handler in handlers)
			{
				var method = handler.GetType().GetMethod("Handle");
				method.Invoke(handler, new[] { @event });
			}

		}
	}
}