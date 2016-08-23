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
		private readonly IUpdatePersonAssociationOnAgentStateReadModel _updatePersonAssociation;

		public AgentStateReadModelCleaner(IAgentStateReadModelPersister persister, IUpdatePersonAssociationOnAgentStateReadModel updatePersonAssociation)
		{
			_persister = persister;
			_updatePersonAssociation = updatePersonAssociation;
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
				_updatePersonAssociation.Update(@event);
			else
				_persister.Delete(@event.PersonId);
		}

	}
}