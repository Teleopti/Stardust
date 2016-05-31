using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelUpdater :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister agentStateReadModelPersister)
		{
			_agentStateReadModelPersister = agentStateReadModelPersister;
		}

		public void Update(AgentStateReadModel model)
		{
			_agentStateReadModelPersister.Persist(model);
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				return;
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}
		
	}
}