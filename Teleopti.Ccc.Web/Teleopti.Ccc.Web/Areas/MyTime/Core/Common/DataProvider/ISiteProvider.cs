using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public interface ISiteProvider
	{
		IEnumerable<ISite> GetShowListSites(DateOnly date, string functionPath);

		IEnumerable<ITeam> GetPermittedTeamsUnderSite(Guid siteId, DateOnly date, string functionPath);
	}
}