using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface IGetTeamAdherence
	{
		IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId);
	}
}