using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateStreamSynchronizer
	{
		private readonly RtaProcessor _processor;
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly ResolveEventHandlers _resolver;
		private readonly IStateContextLoader _stateContextLoader;

		public StateStreamSynchronizer(
			RtaProcessor processor,
			IAgentStateReadModelPersister agentStateReadModelPersister,
			IEventPublisherScope eventPublisherScope,
			IEnumerable<IInitializeble> initializebles,
			ResolveEventHandlers resolver,
			IStateContextLoader stateContextLoader
			)
		{
			_processor = processor;
			_agentStateReadModelPersister = agentStateReadModelPersister;
			_eventPublisherScope = eventPublisherScope;
			_initializebles = initializebles;
			_resolver = resolver;
			_stateContextLoader = stateContextLoader;
		}
		
		public virtual void Initialize()
		{
			var states = _agentStateReadModelPersister.GetActualAgentStates();
			_initializebles.ForEach(s =>
			{
				if (!s.Initialized())
					processStatesTo(s, states);
			});
		}

		private void processStatesTo(object handler, IEnumerable<AgentStateReadModel> states)
		{
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishTo(_resolver, handler)))
			{
				_stateContextLoader.ForSynchronize(states, context =>
				{
					_processor.Process(context);
				});
			}
		}
	}

}