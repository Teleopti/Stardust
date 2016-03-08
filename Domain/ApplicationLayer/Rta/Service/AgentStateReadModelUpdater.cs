using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

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

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		[AnalyticsUnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}

		[UseOnToggle(Toggles.RTA_TerminatedPersons_36042)]
		[AnalyticsUnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				return;
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}




		public void Update(StateInfo info)
		{
			_agentStateReadModelPersister.Persist(info.MakeAgentStateReadModel());
		}
	}
}