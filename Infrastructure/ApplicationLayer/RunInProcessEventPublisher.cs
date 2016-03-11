using System.Collections.Generic;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
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
			var tasks = new List<Task>();
			foreach (var @event in events)
			{
				tasks.Add(Task.Factory.StartNew(() =>
				{
					using (var scope = _resolve.NewScope())
					{
						//fix soon - hard coded for now
						var handler = scope.Resolve(typeof (IntradayOptimizationEventHandler));
						var method = _resolver.HandleMethodFor(handler.GetType(), @event);
						method.Invoke(handler, new[] {@event});
					}
				}));
			}
			Task.WaitAll(tasks.ToArray());
		}
	}
}