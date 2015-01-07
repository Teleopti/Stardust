using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ITeamAdherencePersister
	{
		void Persist(TeamAdherenceReadModel model);
		TeamAdherenceReadModel Get(Guid teamId);
		IEnumerable<TeamAdherenceReadModel> GetForSite(Guid siteId);
	}
}