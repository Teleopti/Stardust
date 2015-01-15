using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class SiteOutOfAdherenceReadModelUpdater : IHandleEvent<PersonOutOfAdherenceEvent>, IHandleEvent<PersonInAdherenceEvent>
	{
		private readonly ISiteOutOfAdherenceReadModelPersister _persister;

		public SiteOutOfAdherenceReadModelUpdater(ISiteOutOfAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			var model = _persister.Get(@event.SiteId) ?? 
				new SiteOutOfAdherenceReadModel {SiteId = @event.SiteId, BusinessUnitId = @event.BusinessUnitId};
			model.PersonIds += @event.PersonId;
			model.Count++;
			_persister.Persist(model);
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			var model = _persister.Get(@event.SiteId);
			if (model == null)
			{
				_persister.Persist(new SiteOutOfAdherenceReadModel() { SiteId = @event.SiteId, BusinessUnitId = @event.BusinessUnitId });
				return;
			}
			if (!model.PersonIds.Contains(@event.PersonId.ToString()))
				return;
			model.PersonIds = model.PersonIds.Replace(@event.PersonId.ToString(), "");
			model.Count--;
			_persister.Persist(model);
		}

	}
}