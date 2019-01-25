using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class SyncInFatClientProcessEventPublisher_SimulateNoNewThreads : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		public SyncInFatClientProcessEventPublisher_SimulateNoNewThreads(ResolveEventHandlers resolver, CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
#pragma warning disable 618
			foreach (var @event in events)
			{
				foreach (var type in _resolver.HandlerTypesFor<IRunInSyncInFatClientProcess>(@event))
				{
					_processor.Process(@event, type);
				}
			}
#pragma warning restore 618
		}
	}
}