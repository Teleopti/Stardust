using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class EventPublisher : IEventPublisher, IPublishEventsFromEventHandlers
	{
		private readonly IResolve _resolver;
		private readonly IEventContextPopulator _eventContextPopulator;

		public EventPublisher(IResolve resolver, IEventContextPopulator eventContextPopulator)
		{
		    _resolver = resolver;
		    _eventContextPopulator = eventContextPopulator;
		}

		public void Publish(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof (IEnumerable<>).MakeGenericType(handlerType);
			var handlers = _resolver.Resolve(enumerableHandlerType) as IEnumerable;
		    if (handlers == null) return;

			_eventContextPopulator.SetMessageDetail(@event);
		    foreach (var handler in handlers)
		    {
		        var method = handler.GetType().GetMethods()
		                            .Single(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
		        method.Invoke(handler, new[] { @event });
		    }
		}
	}
}