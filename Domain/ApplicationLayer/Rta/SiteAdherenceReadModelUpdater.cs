using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	[UseOnToggle(Toggles.RTA_NoBroker_31237)]
	public class SiteAdherenceReadModelUpdater : IHandleEvent<PersonOutOfAdherenceEvent>, IHandleEvent<PersonInAdherenceEvent>
	{
		private readonly ISiteAdherencePersister _siteAdherencePersister;

		public SiteAdherenceReadModelUpdater(ISiteAdherencePersister siteAdherencePersister)
		{
			_siteAdherencePersister = siteAdherencePersister;
		}

		[ReadModelUnitOfWork]
		public void Handle(PersonOutOfAdherenceEvent @event)
		{
			var model = getModel(@event.SiteId);
			model.AgentsOutOfAdherence++;
			_siteAdherencePersister.Persist(model);
		}

		[ReadModelUnitOfWork]
		public void Handle(PersonInAdherenceEvent @event)
		{
			var model = getModel(@event.SiteId);
			if (model.AgentsOutOfAdherence > 0)
			{
				model.AgentsOutOfAdherence--;
				_siteAdherencePersister.Persist(model);
			}
		}

		private SiteAdherenceReadModel getModel(Guid siteId)
		{
			return _siteAdherencePersister.Get(siteId) ?? new SiteAdherenceReadModel() { SiteId = siteId };
		}
	}
}