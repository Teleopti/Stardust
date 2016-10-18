using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	[EnabledBy(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceUpdater : 
		IRunOnHangfire,
		IHandleEvent<PersonOutOfAdherenceEvent>,
		IHandleEvent<PersonInAdherenceEvent>,
		IHandleEvent<PersonNeutralAdherenceEvent>
	{
		private readonly IHistoricalAdherenceReadModelPersister _persister;

		public HistoricalAdherenceUpdater(IHistoricalAdherenceReadModelPersister persister)
		{
			_persister = persister;
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
	}
}
