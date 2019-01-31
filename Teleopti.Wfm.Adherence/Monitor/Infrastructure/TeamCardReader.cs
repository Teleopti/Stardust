using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Wfm.Adherence.Monitor.Infrastructure
{
	public class TeamCardReader : ITeamCardReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;
		private readonly ICurrentBusinessUnit _businessUnit;

		public TeamCardReader(ICurrentUnitOfWork unitOfWork, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId, ICurrentBusinessUnit businessUnit)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
			_businessUnit = businessUnit;
		}

		public IEnumerable<TeamCardModel> Read()
		{
			return read(null, null);
		}

		public IEnumerable<TeamCardModel> Read(IEnumerable<Guid> skillIds)
		{
			return read(null, skillIds);
		}

		public IEnumerable<TeamCardModel> Read(Guid siteId)
		{
			return read(siteId, null);
		}

		public IEnumerable<TeamCardModel> Read(Guid siteId, IEnumerable<Guid> skillIds)
		{
			return read(siteId, skillIds);
		}

		private IEnumerable<TeamCardModel> read(Guid? siteId, IEnumerable<Guid> skillIds)
		{
			var querySite = siteId.HasValue;
			var querySkills = skillIds.EmptyIfNull().Any();

			var siteWhere = "a.SiteId = :siteId AND";

			var skillJoin = @"
					INNER JOIN ReadModel.GroupingReadOnly AS g
					ON a.PersonId = g.PersonId
					";

			var skillWhere = @"
					g.GroupId IN (:skillIds) AND
					g.PageId = :skillGroupingPageId AND 
					:now BETWEEN g.StartDate AND g.EndDate AND
					";

			var query = _unitOfWork.Current()
				.Session()
				.CreateSQLQuery($@"
					SELECT
						a.BusinessUnitId,
						a.SiteId, 
						MAX(a.SiteName) as SiteName,
						MAX(a.TeamName) as TeamName,
						a.TeamId, 
						COUNT(DISTINCT CASE WHEN a.AlarmStartTime <= :now THEN a.PersonId END) as InAlarmCount,
						COUNT(1) as AgentsCount
						
					FROM 
						ReadModel.AgentState AS a WITH(NOLOCK)
					
					{(querySkills ? skillJoin : "")}

					WHERE 
					{(querySkills ? skillWhere : "")}
					{(querySite ? siteWhere : "")}

					a.HasAssociation = 1 AND

					a.BusinessUnitId = :businessUnitId

					GROUP BY 
						a.BusinessUnitId,
						a.SiteId, 
						a.TeamId
					");

			query
				.SetParameter("now", _now.UtcDateTime())
				.SetParameter("businessUnitId", _businessUnit.Current().Id.Value)
				;

			if (querySite)
				query
					.SetParameter("siteId", siteId);

			if (querySkills)
				query
					.SetParameterList("skillIds", skillIds)
					.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
					;
			
			return query
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamCardModel)))
				.List()
				.Cast<TeamCardModel>();
		}
	}
}