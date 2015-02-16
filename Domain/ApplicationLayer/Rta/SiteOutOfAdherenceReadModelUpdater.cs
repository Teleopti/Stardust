using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class SiteOutOfAdherenceReadModelUpdater : 
		IHandleEvent<PersonOutOfAdherenceEvent>, 
		IHandleEvent<PersonInAdherenceEvent>,
		IInitializeble,
		IRecreatable
	{
		private readonly ISiteOutOfAdherenceReadModelPersister _persister;

		public SiteOutOfAdherenceReadModelUpdater(ISiteOutOfAdherenceReadModelPersister persister)
		{
			_persister = persister;
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonOutOfAdherenceEvent @event)
		{
			handleModel(@event.BusinessUnitId, @event.SiteId, @event.PersonId, 1, model =>
			{
				model.Count = getCount(model.State);
				_persister.Persist(model);
			});
		}

		[ReadModelUnitOfWork]
		public virtual void Handle(PersonInAdherenceEvent @event)
		{
			SiteOutOfAdherenceReadModel model = null;
			handleModel(@event.BusinessUnitId, @event.SiteId, @event.PersonId, -1, m =>
			{
				if (m.State.All(x => x.PersonId != @event.PersonId))
					return;
				m.Count = getCount(m.State);
				model = m;
			});
			if (model !=null)
				_persister.Persist(model);
		}

		private void handleModel(Guid businessUnitId, Guid siteId, Guid personId, int adhCount, Action<SiteOutOfAdherenceReadModel> mutate)
		{
			var model = _persister.Get(siteId);
			var adherneceModel = new SiteOutOfAdherenceReadModelState() {Count = adhCount,PersonId = personId};
			if (model == null)
			{
				model = new SiteOutOfAdherenceReadModel()
				{
					SiteId = siteId,
					BusinessUnitId = businessUnitId,
					State = new[] { adherneceModel }
				};
			}
			else
				model.State = model.State.Concat(new[] { adherneceModel });
			mutate(model);
		}

		private static int getCount(IEnumerable<SiteOutOfAdherenceReadModelState> states)
		{
			return ((from o in states
				group o by o.PersonId into g
				select new
				{
					count = g.Sum(a => a.Count)
				}).Where(c => c.count > 0)).Count();
		}

		[ReadModelUnitOfWork]
		public virtual bool Initialized()
		{
			return _persister.HasData();
		}

		[ReadModelUnitOfWork]
		public virtual void DeleteAll()
		{
			_persister.Clear();
		}
	}
}