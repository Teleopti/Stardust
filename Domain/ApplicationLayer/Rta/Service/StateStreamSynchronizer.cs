using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateStreamSynchronizer
	{
		private readonly INow _now;
		private readonly RtaProcessor _processor;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly ResolveEventHandlers _resolver;

		public StateStreamSynchronizer(
			INow now,
			RtaProcessor processor,
			IAgentStateReadModelReader agentStateReadModelReader,
			IEventPublisherScope eventPublisherScope,
			IEnumerable<IInitializeble> initializebles,
			ResolveEventHandlers resolver
			)
		{
			_now = now;
			_processor = processor;
			_agentStateReadModelReader = agentStateReadModelReader;
			_eventPublisherScope = eventPublisherScope;
			_initializebles = initializebles;
			_resolver = resolver;
		}
		
		public virtual void Initialize()
		{
			var states = _agentStateReadModelReader.GetActualAgentStates();
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
				states.ForEach(s =>
				{
					var context = new RtaProcessContext(
						new ExternalUserStateInputModel
						{
							StateCode = s.StateCode,
							PlatformTypeId = s.PlatformTypeId.ToString()
						},
						new PersonOrganizationData
						{
							BusinessUnitId = s.BusinessUnitId,
							PersonId = s.PersonId,
							SiteId = s.SiteId.GetValueOrDefault(),
							TeamId = s.TeamId.GetValueOrDefault()
						},
						_now,
						null,
						new EmptyPreviousStateInfoLoader()
						);
					_processor.Process(context);
				});
			}
		}
	}

}