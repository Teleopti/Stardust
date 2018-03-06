using System.Linq;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public abstract class InProcessEventPublisher<T> : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;

		protected InProcessEventPublisher(ResolveEventHandlers resolver, CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			Task.WaitAll((
					from @event in events
#pragma warning disable 618
					from handlerType in _resolver.HandlerTypesFor<T>(@event)
					select Task.Run(() =>
							_processor.Process(@event, handlerType)
#pragma warning restore 618
					))
				.ToArray());
		}
	}
}