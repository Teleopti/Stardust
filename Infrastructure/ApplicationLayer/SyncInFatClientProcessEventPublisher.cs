using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncInFatClientProcessEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public SyncInFatClientProcessEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			Task.WaitAll((
				from @event in events
				from handlerType in _resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(@event)
				select Task.Run(() =>
					_processor.Process(@event, handlerType)
					))
				.ToArray());
		}
	}
}
