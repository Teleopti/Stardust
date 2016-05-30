using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelUpdater :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>,
		IAgentStateReadModelUpdater
	{
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;
		private readonly IAgentStatePersister _agentStatePersister;

		public AgentStateReadModelUpdater(
			IAgentStateReadModelPersister agentStateReadModelPersister,
			IAgentStatePersister agentStatePersister
			)
		{
			_agentStateReadModelPersister = agentStateReadModelPersister;
			_agentStatePersister = agentStatePersister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_agentStateReadModelPersister.Delete(@event.PersonId);
			_agentStatePersister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				return;
			_agentStateReadModelPersister.Delete(@event.PersonId);
			_agentStatePersister.Delete(@event.PersonId);
		}




		public void UpdateState(Context info)
		{
			_agentStatePersister.Persist(info.MakeAgentState());
		}

		public void UpdateReadModel(Context info)
		{
			_agentStateReadModelPersister.Persist(info.MakeAgentStateReadModel());
		}
	}
}