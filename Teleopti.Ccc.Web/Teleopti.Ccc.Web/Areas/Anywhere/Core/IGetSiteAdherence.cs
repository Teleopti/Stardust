using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface IGetSiteAdherence
	{
		IEnumerable<SiteOutOfAdherence> ReadAdherenceForAllSites(Guid businessUnitId);
	}
}