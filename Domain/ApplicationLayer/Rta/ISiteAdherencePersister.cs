using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ISiteAdherencePersister
	{
		void Persist(SiteAdherenceReadModel model);
		SiteAdherenceReadModel Get(Guid siteId);
	}
}