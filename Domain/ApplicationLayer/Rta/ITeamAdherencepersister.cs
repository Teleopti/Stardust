using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface ITeamAdherencepersister
	{
		void Persist(TeamAdherenceReadModel model);
		TeamAdherenceReadModel Get(Guid teamId);
	}
}