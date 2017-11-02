using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.TestCommon.FakeRepositories
{
	public class FakeAnalyticsSiteRepository : IAnalyticsSiteRepository
	{
		private readonly IList<AnalyticsSite> _analyticsSites = new List<AnalyticsSite>();
		public void Has(AnalyticsSite site)
		{
			_analyticsSites.Add(site);
		}

		public IList<AnalyticsSite> GetSites()
		{
			return _analyticsSites;
		}

		public void UpdateName(Guid siteCode, string name)
		{
			var site = _analyticsSites.FirstOrDefault(x => x.SiteCode == siteCode);
			site.Name = name;
		}
	}
}