using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public class AgentStateMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonDeletedEvent>,
		IHandleEvent<PersonAssociationChangedEvent>
	{
		private readonly IAgentStatePersister _persister;

		public AgentStateMaintainer(IAgentStatePersister persister)
		{
			_persister = persister;
		}

		[UnitOfWork]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
		}

		[UnitOfWork]
		public virtual void Handle(PersonAssociationChangedEvent @event)
		{
			if (!@event.TeamId.HasValue)
			{
				_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
				return;
			}
			if (@event.ExternalLogons.IsNullOrEmpty())
			{
				_persister.Delete(@event.PersonId, DeadLockVictim.Yes);
				return;
			}

			_persister.Prepare(new AgentStatePrepare
			{
				PersonId = @event.PersonId,
				BusinessUnitId = @event.BusinessUnitId.GetValueOrDefault(),
				SiteId = @event.SiteId,
				TeamId = @event.TeamId
			}, DeadLockVictim.Yes);
		}

	}
}