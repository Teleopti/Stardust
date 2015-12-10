using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetTeamAdherence
	{
		IEnumerable<TeamViewModel> ForSite(string siteId);
		TeamOutOfAdherence GetOutOfAdherence(string teamId);
		IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(string siteId);
	}
}