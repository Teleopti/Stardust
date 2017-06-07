using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class TeamsInAlarmReader : ITeamsInAlarmReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;
		private readonly HardcodedSkillGroupingPageId _hardcodedSkillGroupingPageId;

		public TeamsInAlarmReader(ICurrentUnitOfWork unitOfWork, INow now, HardcodedSkillGroupingPageId hardcodedSkillGroupingPageId)
		{
			_unitOfWork = unitOfWork;
			_now = now;
			_hardcodedSkillGroupingPageId = hardcodedSkillGroupingPageId;
		}

		public IEnumerable<TeamInAlarmModel> Read()
		{
			return read(null, null);
		}

		public IEnumerable<TeamInAlarmModel> Read(IEnumerable<Guid> skillIds)
		{
			return read(null, skillIds);
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
		{
			return read(siteId, null);
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId, IEnumerable<Guid> skillIds)
		{
			return read(siteId, skillIds);
		}

		private IEnumerable<TeamInAlarmModel> read(Guid? siteId, IEnumerable<Guid> skillIds)
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
						a.TeamId, 
						COUNT(DISTINCT CASE WHEN a.AlarmStartTime <= :now THEN a.PersonId END) as InAlarmCount
					FROM 
						ReadModel.AgentState AS a WITH(NOLOCK)
					
					{(querySkills ? skillJoin : "")}

					WHERE 
					{(querySkills ? skillWhere : "")}
					{(querySite ? siteWhere : "")}

					a.IsDeleted = 0

					GROUP BY 
						a.BusinessUnitId,
						a.SiteId, 
						a.TeamId
					");

			query
				.SetParameter("now", _now.UtcDateTime())
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
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamInAlarmModel)))
				.List()
				.Cast<TeamInAlarmModel>();
		}
	}
}