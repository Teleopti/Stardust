using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ISiteOutOfAdherenceReadModelReader
	{
		IEnumerable<SiteOutOfAdherenceReadModel> Read(Guid businessUnitId);
	}

	public interface ISiteOutOfAdherenceReadModelPersister
	{
		void Persist(SiteOutOfAdherenceReadModel model);
		SiteOutOfAdherenceReadModel Get(Guid siteId);
		void Clear();
		bool HasData();
	}
}