using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public NumberOfAgentsInSiteReader(ICurrentUnitOfWork currentUnitOfWork, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}

		public IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds)
		{
			if (siteIds.IsEmpty())
				return new Dictionary<Guid, int>();
			var models =
				_currentUnitOfWork.Session().CreateSQLQuery(@"
SELECT
	Site as 'SiteId',
	count(Parent) as 'AgentsCount'
FROM dbo.v_PersonPeriodTeamSiteBu WITH(NOEXPAND)
WHERE :now BETWEEN StartDate AND EndDate 
AND Site in (:sites)
group by Site")
					.SetDateTime("now", _now.UtcDateTime().Date)
					.SetParameterList("sites", siteIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof(siteViewModel)))
					.List()
					.Cast<siteViewModel>();

			var initializedSites =
				from siteId in siteIds
				where !models.Select(x => x.SiteId).Contains(siteId)
				select new siteViewModel { SiteId = siteId, AgentsCount = 0 };

			return initializedSites.Concat(models).ToDictionary(x => x.SiteId, y => y.AgentsCount);
		}

		public IDictionary<Guid, int> Read(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{
			if (siteIds.IsEmpty())
				return new Dictionary<Guid, int>();
			var models =
				_currentUnitOfWork.Session()
					.CreateSQLQuery(@"
SELECT
	pp.Site as 'SiteId',
	count(DISTINCT pp.Parent) as 'AgentsCount'
FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND)

INNER JOIN ReadModel.GroupingReadOnly AS g
ON pp.Parent = g.PersonId					
WHERE g.GroupId IN (:skillIds)
AND g.PageId = :skillGroupingPageId
AND :now BETWEEN g.StartDate AND g.EndDate
					
AND pp.Site in (:sites)
GROUP BY pp.Site
")
					.SetDateTime("now", _now.UtcDateTime().Date)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					.SetParameterList("sites", siteIds)
					.SetParameterList("skillIds", skillIds)

					.SetResultTransformer(Transformers.AliasToBean(typeof(siteViewModel)))
					.List()
					.Cast<siteViewModel>();

			return models.ToDictionary(x => x.SiteId, y => y.AgentsCount);
		}
		
		private class siteViewModel
		{
			public Guid SiteId { get; set; }
			public int AgentsCount { get; set; }
		}
	}
}