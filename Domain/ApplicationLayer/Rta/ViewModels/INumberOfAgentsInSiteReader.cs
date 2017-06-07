using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels
{
	public interface INumberOfAgentsInSiteReader
	{
		IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds);
		IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds);
	}
}
