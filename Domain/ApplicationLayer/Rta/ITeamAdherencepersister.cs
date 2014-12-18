using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ITeamAdherencePersister
	{
		void Persist(TeamAdherenceReadModel model);
		TeamAdherenceReadModel Get(Guid teamId);
	}
}