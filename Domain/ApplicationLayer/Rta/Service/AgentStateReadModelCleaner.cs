using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateReadModelCleaner :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStateReadModelPersister _persister;

		public AgentStateReadModelCleaner(IAgentStateReadModelPersister persister)
		{
			_persister = persister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (@event.TeamId.HasValue)
				_persister.UpdateAssociation(@event.PersonId, @event.TeamId.Value, @event.SiteId);
			else
				_persister.Delete(@event.PersonId);
		}
	}
}