using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Wfm.Adherence.States
{
	public class AgentStateMaintainer :
		IRunOnHangfire,
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

		public void Handle(IEnumerable<IEvent> events)
		{
			events.OfType<PersonAssociationChangedEvent>()
				.ForEach(Prepare);
		}
		
		[UnitOfWork]
		protected virtual void Prepare(PersonAssociationChangedEvent @event)
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