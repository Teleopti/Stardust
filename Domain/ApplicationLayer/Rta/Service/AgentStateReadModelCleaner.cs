using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
	public class AgentStateReadModelUpdaterWithAssociationUpdate : AgentStateReadModelCleaner
	{
		public AgentStateReadModelUpdaterWithAssociationUpdate(IAgentStateReadModelPersister persister) 
			: base(persister)
		{
		}

		public override void UpdateAssociation(PersonAssociationChangedEvent @event)
		{
			Persister.UpdateAssociation(@event.PersonId, @event.TeamId.Value, @event.SiteId);
		}
	}

	[DisabledBy(Toggles.RTA_RemoveSiteTeamOutOfAdherenceReadModels_40069)]
	public class AgentStateReadModelCleaner :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		protected readonly IAgentStateReadModelPersister Persister;

		public AgentStateReadModelCleaner(IAgentStateReadModelPersister persister)
		{
			Persister = persister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			Persister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				UpdateAssociation(@event);
			else
				Persister.Delete(@event.PersonId);
		}

		public virtual void UpdateAssociation(PersonAssociationChangedEvent @event)
		{
			
		}
	}
}