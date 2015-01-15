using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ISiteOutOfAdherenceReadModelPersister
	{
		void Persist(SiteOutOfAdherenceReadModel model);
		SiteOutOfAdherenceReadModel Get(Guid siteId);
		IEnumerable<SiteOutOfAdherenceReadModel> GetForBusinessUnit(Guid businessUnitId);
		void Clear();
		bool HasData();
	}
}