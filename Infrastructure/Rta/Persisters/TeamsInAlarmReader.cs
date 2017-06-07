using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
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
			throw new NotImplementedException();
		}

		public IEnumerable<TeamInAlarmModel> Read(IEnumerable<Guid> skillIds)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT SiteId, TeamId, COUNT(*) AS Count
					FROM ReadModel.AgentState WITH(NOLOCK)
					WHERE AlarmStartTime <= :now
					AND SiteId = :siteId
					AND IsDeleted = 0
					GROUP BY TeamId
					")
				.SetParameter("siteId", siteId)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamInAlarmModel)))
				.List()
				.Cast<TeamInAlarmModel>();
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId, IEnumerable<Guid> skillIds)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT
						a.SiteId, 
						a.TeamId, 
						COUNT(DISTINCT a.PersonId) AS Count
					FROM 
						ReadModel.AgentState AS a WITH(NOLOCK)
					
					INNER JOIN ReadModel.GroupingReadOnly AS g
					ON a.PersonId = g.PersonId

					WHERE g.GroupId IN (:skillIds)
					AND g.PageId = :skillGroupingPageId
					AND :now BETWEEN g.StartDate AND g.EndDate

					AND a.AlarmStartTime <= :now
					AND a.SiteId = :siteId
					AND IsDeleted = 0

					GROUP BY 
						a.TeamId
					")
				.SetParameter("siteId", siteId)
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("skillIds", skillIds)
				.SetParameter("skillGroupingPageId", _hardcodedSkillGroupingPageId.Get())
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamInAlarmModel)))
				.List()
				.Cast<TeamInAlarmModel>();
		}

	}
}