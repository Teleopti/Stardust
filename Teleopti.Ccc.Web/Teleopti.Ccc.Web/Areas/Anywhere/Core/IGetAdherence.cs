using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetAdherence
	{
		IEnumerable<TeamViewModel> ForSite(string siteId);

		TeamOutOfAdherence GetOutOfAdherence(string teamId);

		IEnumerable<TeamOutOfAdherence> GetOutOfAdherenceForTeamsOnSite(string siteId);

		Guid GetBusinessUnitId(string teamId);
	}
}