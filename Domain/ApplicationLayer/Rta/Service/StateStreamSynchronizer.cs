using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateStreamSynchronizer
	{
		void Sync(string tenant);
		void Initialize(string tenant);
	}

	public class NoStateStreamSynchronizer : IStateStreamSynchronizer
	{
		public void Sync(string tenant)
		{
		}

		public void Initialize(string tenant)
		{
		}
	}

	public class StateStreamSynchronizer : IStateStreamSynchronizer
	{
		private readonly INow _now;
		private readonly RtaProcessor _processor;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IAgentStateReadModelReader _agentStateReadModelReader;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly IEnumerable<IRecreatable> _recreatables;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;

		public StateStreamSynchronizer(
			INow now,
			RtaProcessor processor,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateReadModelReader agentStateReadModelReader,
			AgentStateAssembler agentStateAssembler,
			IEventPublisherScope eventPublisherScope,
			IEnumerable<IInitializeble> initializebles,
			IEnumerable<IRecreatable> recreatables,
			IDistributedLockAcquirer distributedLockAcquirer
			)
		{
			_now = now;
			_processor = processor;
			_personOrganizationProvider = personOrganizationProvider;
			_agentStateReadModelReader = agentStateReadModelReader;
			_agentStateAssembler = agentStateAssembler;
			_eventPublisherScope = eventPublisherScope;
			_initializebles = initializebles;
			_recreatables = recreatables;
			_distributedLockAcquirer = distributedLockAcquirer;
		}

		public void Sync(string tenant)
		{
			var currentTime = _now.UtcDateTime();
			var states = _agentStateReadModelReader.GetActualAgentStates(tenant);
			_recreatables.ForEach(s =>
			{
				s.DeleteAll();
				processStatesTo(s, states, currentTime, tenant);
			});
		}

		public virtual void Initialize(string tenant)
		{
			var currentTime = _now.UtcDateTime();
			var states = _agentStateReadModelReader.GetActualAgentStates(tenant);
			_initializebles.ForEach(s =>
			{
				if (!s.Initialized())
					processStatesTo(s, states, currentTime, tenant);
			});
		}

		private void processStatesTo(object handler, IEnumerable<AgentStateReadModel> states, DateTime currentTime, string tenant)
		{	
			using (_distributedLockAcquirer.LockForTypeOf(handler))
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishTo(handler)))
			{
				states.ForEach(s =>
				{
					var context = new RtaProcessContext(
						null,
						s.PersonId,
						s.BusinessUnitId,
						currentTime,
						_personOrganizationProvider,
						null,
						null, 
						null,
						() => _agentStateAssembler.MakeEmpty(s.PersonId),
						(a, b) => _agentStateAssembler.MakeCurrentStateFromPrevious(s),
						tenant
						);
					_processor.Process(context);
				});
			}
		}
	}

}