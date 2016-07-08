using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateStreamSynchronizer
	{
		private readonly RtaProcessor _processor;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly ResolveEventHandlers _resolver;
		private readonly IContextLoader _contextLoader;

		public StateStreamSynchronizer(
			RtaProcessor processor,
			IEventPublisherScope eventPublisherScope,
			IEnumerable<IInitializeble> initializebles,
			ResolveEventHandlers resolver,
			IContextLoader contextLoader
			)
		{
			_processor = processor;
			_eventPublisherScope = eventPublisherScope;
			_initializebles = initializebles;
			_resolver = resolver;
			_contextLoader = contextLoader;
		}
		
		public virtual void Initialize()
		{
			_initializebles.ForEach(s =>
			{
				if (!s.Initialized())
					processStatesTo(s);
			});
		}

		private void processStatesTo(object handler)
		{
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishTo(_resolver, handler)))
			{
				_contextLoader.ForSynchronize(context =>
				{
					_processor.Process(context);
				});
			}
		}
	}

}