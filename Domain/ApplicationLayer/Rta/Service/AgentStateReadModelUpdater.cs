using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelUpdater :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IAgentStateReadModelUpdater
	{
		private readonly IAgentStateReadModelPersister _agentStateReadModelPersister;

		public AgentStateReadModelUpdater(IAgentStateReadModelPersister agentStateReadModelPersister)
		{
			_agentStateReadModelPersister = agentStateReadModelPersister;
		}

		[UseOnToggle(Toggles.RTA_DeletedPersons_36041)]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_agentStateReadModelPersister.Delete(@event.PersonId);
		}

		public void Update(StateInfo info)
		{
			_agentStateReadModelPersister.PersistActualAgentReadModel(info.MakeAgentStateReadModel());
		}
	}
}