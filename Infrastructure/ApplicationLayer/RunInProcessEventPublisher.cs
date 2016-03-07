using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class RunInProcessEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;

		public RunInProcessEventPublisher(ResolveEventHandlers resolver)
		{
			_resolver = resolver;
		}

		public void Publish(params IEvent[] events)
		{
			var task = new Task(() =>
			{
				foreach (var @event in events)
				{
					var handlers = _resolver.ResolveInProcessForEvent(@event);
					foreach (var handler in handlers)
					{
						var method = _resolver.HandleMethodFor(handler.GetType(), @event);
						method.Invoke(handler, new[] { @event });
					}
				}
			});
			task.Start();
			//Task.Factory.StartNew(() => task);
			Task.WaitAll(task);
		}
	}
}