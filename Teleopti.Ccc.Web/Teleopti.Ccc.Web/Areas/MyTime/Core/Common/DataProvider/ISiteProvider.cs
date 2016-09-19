using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ISiteProvider
	{
		IEnumerable<ISite> GetPermittedSites(DateOnly date, string functionPath);

		IEnumerable<ITeam> GetTeamsUnderSite(Guid siteId);
	}
}