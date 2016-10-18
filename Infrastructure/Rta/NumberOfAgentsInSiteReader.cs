using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Mapping;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class NumberOfAgentsInSiteReader : INumberOfAgentsInSiteReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;
		private const string sqlQuery = @"
SELECT
	a.Site as 'SiteId',
	count(a.Parent) as 'NumberOfAgents'
FROM
(
	SELECT
	pp.StartDate,
	pp.Parent,
	pp.PersonPeriod,
	pp.BusinessUnit,
	pp.Site,
	pp.Team,
	ROW_NUMBER()OVER(PARTITION BY pp.Parent ORDER BY pp.StartDate DESC) as is_current
	FROM dbo.v_PersonPeriodTeamSiteBu pp WITH(NOEXPAND)
	WHERE pp.StartDate <= :now 
) a
inner join person p on 
a.parent = p.id

{0} a.is_current=1
and a.Site in (:sites)
and (p.TerminalDate is null or p.TerminalDate > :now)
group by a.Site";


		public NumberOfAgentsInSiteReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IDictionary<Guid, int> FetchNumberOfAgents(IEnumerable<Guid> siteIds)
		{
			var models =
				_currentUnitOfWork.Session().CreateSQLQuery(string.Format(sqlQuery, " WHERE "))
					.SetDateTime("now", _now.UtcDateTime())
					.SetParameterList("sites", siteIds)
					.SetResultTransformer(Transformers.AliasToBean(typeof(siteViewModel)))
					.List()
					.Cast<siteViewModel>();
			return initializeAndConcat(siteIds, models);
		}

		public IDictionary<Guid, int> ForSkills(IEnumerable<Guid> siteIds, IEnumerable<Guid> skillIds)
		{
			var models =
				_currentUnitOfWork.Session()
					.CreateSQLQuery(
						string.Format(sqlQuery, @"
						INNER JOIN ReadModel.GroupingReadOnly AS g
						ON p.Id = g.PersonId					
						WHERE g.GroupId IN (:skillIds)
						AND g.PageId = :skillGroupingPageId
						AND "))
					.SetDateTime("now", _now.UtcDateTime())
					.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
					.SetParameterList("sites", siteIds)
					.SetParameterList("skillIds", skillIds)

					.SetResultTransformer(Transformers.AliasToBean(typeof(siteViewModel)))
					.List()
					.Cast<siteViewModel>();

			return models.ToDictionary(x => x.SiteId, y => y.NumberOfAgents);
		}

		private IDictionary<Guid, int> initializeAndConcat(IEnumerable<Guid> siteIds, IEnumerable<siteViewModel> models)
		{
			var initializedSites =
				from siteId in siteIds
				where !models.Select(x => x.SiteId).Contains(siteId)
				select new siteViewModel {SiteId = siteId, NumberOfAgents = 0};

			return  initializedSites.Concat(models).ToDictionary(x => x.SiteId, y => y.NumberOfAgents);
		}


		private class siteViewModel
		{
			public Guid SiteId { get; set; }
			public int NumberOfAgents { get; set; }
		}
	}
}