using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public interface ITeamOutOfAdherenceReadModelReader
	{
		IEnumerable<TeamOutOfAdherenceReadModel> Read(Guid siteId);
	}

	public interface ITeamOutOfAdherenceReadModelPersister
	{
		void Persist(TeamOutOfAdherenceReadModel model);
		TeamOutOfAdherenceReadModel Get(Guid teamId);
		bool HasData();
		void Clear();
	}
}