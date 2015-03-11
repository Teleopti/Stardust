using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.DistributedLock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IStateStreamSynchronizer
	{
		void Sync();
		void Initialize();
	}

	public class NoStateStreamSynchronizer : IStateStreamSynchronizer
	{
		public void Sync()
		{
		}

		public void Initialize()
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
		
		public void Sync()
		{
			var currentTime = _now.UtcDateTime();
			var states = _agentStateReadModelReader.GetActualAgentStates();
			_recreatables.ForEach(s =>
			{
				s.DeleteAll();
				processStatesTo(s, states, currentTime);
			});
		}

		public virtual void Initialize()
		{
			var currentTime = _now.UtcDateTime();
			var states = _agentStateReadModelReader.GetActualAgentStates();
			_initializebles.ForEach(s =>
			{
				if (!s.Initialized())
					processStatesTo(s, states, currentTime);
			});
		}

		private void processStatesTo(object handler, IEnumerable<AgentStateReadModel> states, DateTime currentTime)
		{	
			using (_distributedLockAcquirer.LockForTypeOf(handler))
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishToSingleHandler(handler)))
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
						(a, b) => _agentStateAssembler.MakeCurrentStateFromPrevious(s)
						);
					_processor.Process(context);
				});
			}
		}
	}

}