﻿using System.Collections;
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
	    private readonly ICurrentIdentity _currentIdentity;

	    public EventPublisher(IResolve resolver, ICurrentIdentity currentIdentity)
		{
		    _resolver = resolver;
		    _currentIdentity = currentIdentity;
		}

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Publish(IEvent @event)
		{
			var handlerType = typeof(IHandleEvent<>).MakeGenericType(@event.GetType());
			var enumerableHandlerType = typeof (IEnumerable<>).MakeGenericType(handlerType);
			var handlers = _resolver.Resolve(enumerableHandlerType) as IEnumerable;
		    if (handlers == null) return;

		    @event.SetMessageDetail(_currentIdentity);
		    foreach (var handler in handlers)
		    {
		        var method = handler.GetType().GetMethods()
		                            .Single(m => m.Name == "Handle" && m.GetParameters().Single().ParameterType == @event.GetType());
		        method.Invoke(handler, new[] { @event });
		    }
		}
	}
}