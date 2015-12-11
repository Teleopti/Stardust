using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetTeamAdherence
	{
		IEnumerable<TeamViewModel> ForSite(Guid siteId);
		TeamOutOfAdherence GetOutOfAdherence(Guid teamId);
		IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(Guid siteId);
	}
}