using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class SyncAllEventPublisher : IEventPublisher
	{
		private readonly ResolveEventHandlers _resolver;
		private readonly CommonEventProcessor _processor;


		public SyncAllEventPublisher(
			ResolveEventHandlers resolver,
			CommonEventProcessor processor)
		{
			_resolver = resolver;
			_processor = processor;
		}

		public void Publish(params IEvent[] events)
		{
			foreach (var @event in events)
			{
				var handlerTypes = _resolver.HandlerTypesFor<IRunOnHangfire>(@event);
				foreach (var handlerType in handlerTypes)
				{
					_processor.Process(@event, handlerType);
				}
			}
			

#pragma warning disable 618
			events.Where(e => _resolver.HandlerTypesFor<IRunOnServiceBus>(e).Any())
#pragma warning restore 618
				.ForEach(ProcessLikeTheBus);
		}

		[AsSystem]
		protected virtual void ProcessLikeTheBus(IEvent @event)
		{
#pragma warning disable 618
			foreach (var handler in _resolver.HandlerTypesFor<IRunOnServiceBus>(@event))
#pragma warning restore 618
				_processor.Process(@event, handler);
		}
	}
}