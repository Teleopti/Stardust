using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Analytics;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAnalyticsSiteRepository
	{
		void UpdateName(Guid siteCode, string name);
		IList<AnalyticsSite> GetSites();
	}
}