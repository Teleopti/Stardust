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
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly IEnumerable<IInitializeble> _initializebles;
		private readonly IEnumerable<IRecreatable> _recreatables;
		private readonly IDistributedLockAcquirer _distributedLockAcquirer;

		public StateStreamSynchronizer(
			INow now,
			RtaProcessor processor,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateReadModelReader agentStateReadModelReader,
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
			using (_eventPublisherScope.OnThisThreadPublishTo(new SyncPublishTo(handler)))
			{
				states.ForEach(s =>
				{
					var context = new RtaProcessContext(
						new ExternalUserStateInputModel
						{
							StateCode = s.StateCode
						},
						s.PersonId,
						s.BusinessUnitId,
						currentTime,
						_personOrganizationProvider,
						null,
						null,
						null,
						new EmptyPreviousStateInfoLoader()
						);
					_processor.Process(context);
				});
			}
		}
	}

}