using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
				from registrationType in registrationTypes
				select Task.Factory.StartNew(() =>
				{
					using (var scope = _resolve.NewScope())
					{
						var handler = scope.Resolve(registrationType);
						var method = _resolver.HandleMethodFor(handler.GetType(), @event);
						method.Invoke(handler, new[] {@event});
					}
				})).ToArray());
		}
	}
}