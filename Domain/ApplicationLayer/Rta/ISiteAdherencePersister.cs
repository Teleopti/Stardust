using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ISiteAdherencePersister
	{
		void Persist(SiteAdherenceReadModel model);
		SiteAdherenceReadModel Get(Guid siteId);
		IEnumerable<SiteAdherenceReadModel> GetAll(Guid businessUnitId);
	}
}