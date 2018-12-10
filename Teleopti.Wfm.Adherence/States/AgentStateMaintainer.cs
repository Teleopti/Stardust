using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.States
{
	public class AgentStateMaintainer :
		IRunOnHangfire,
		IHandleEvent<PersonAssociationChangedEvent>,
		IHandleEvents
	{
		private readonly IAgentStatePersister _persister;

		public AgentStateMaintainer(IAgentStatePersister persister)
		{
			_persister = persister;
		}
		
		public void Subscribe(SubscriptionRegistrator registrator)
		{
			registrator.SubscribeTo<PersonAssociationChangedEvent>();
		}

		[EnabledBy(Toggles.RTA_TooManyPersonAssociationChangedEvents_Packages_78669)]
		public void Handle(IEnumerable<IEvent> events)
		{
			events.OfType<PersonAssociationChangedEvent>()
				.ForEach(Handle);
		}

		[DisabledBy(Toggles.RTA_TooManyPersonAssociationChangedEvents_Packages_78669)]
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