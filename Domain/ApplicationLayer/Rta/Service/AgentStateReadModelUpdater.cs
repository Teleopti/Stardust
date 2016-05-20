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

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister agentStateReadModelPersister)
		{
			_agentStateReadModelPersister = agentStateReadModelPersister;
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}

		[AnalyticsUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				return;
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}




		public void Update(Context info)
		{
			_agentStateReadModelPersister.Persist(info.MakeAgentStateReadModel());
		}
	}
}