using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>,
		IHandleEvent<TenantDayTickEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _persister;
		private readonly INow _now;

		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister persister, INow now)
		{
			_persister = persister;
			_now = now;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			_persister.AddOut(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			_persister.AddIn(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonNeutralAdherenceEvent @event)
		{
			_persister.AddNeutral(@event.PersonId, @event.Timestamp);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(TenantDayTickEvent tenantDayTickEvent)
		{
			_persister.Remove(_now.UtcDateTime().Date.AddDays(-1));
		}
	}
}
