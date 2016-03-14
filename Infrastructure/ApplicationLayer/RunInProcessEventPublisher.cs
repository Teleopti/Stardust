using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class RunInProcessEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly IResolve _resolve;

		public RunInProcessEventPublisher(ResolveEventHandlers resolver, IResolve resolve)
		{
			_resolver = resolver;
			_resolve = resolve;
		}

		public void Publish(params IEvent[] events)
		{
			Task.WaitAll((from @event in events
				let handlerType = typeof (IHandleEvent<>).MakeGenericType(@event.GetType())
				let registrationTypes = _resolve.ConcreteTypesFor(handlerType).Where(x => x.GetInterfaces().Contains(typeof (IRunInProcess)))
				where registrationTypes.Count() == 1 //only supports one event handler impl currently - struggle to get correct handlertype if multiple
				select Task.Factory.StartNew(() =>
				{
					using (var scope = _resolve.NewScope())
					{
						var handler = scope.Resolve(handlerType);
						var method = _resolver.HandleMethodFor(handler.GetType(), @event);
						method.Invoke(handler, new[] {@event});
					}
				})).ToArray());
		}
	}
}