using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ITeamAdherencePersister
	{
		void Persist(TeamAdherenceReadModel model);
		TeamAdherenceReadModel Get(Guid teamId);
	}

	public interface ISiteAdherencePersister
	{
		void Persist(SiteAdherenceReadModel model);
		SiteAdherenceReadModel Get(Guid siteId);
	}
}