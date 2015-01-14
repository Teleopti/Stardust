using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ITeamOutOfAdherenceReadModelPersister
	{
		void Persist(TeamOutOfAdherenceReadModel model);
		TeamOutOfAdherenceReadModel Get(Guid teamId);
		IEnumerable<TeamOutOfAdherenceReadModel> GetForSite(Guid siteId);
		bool HasData();
		void Clear();
	}
}