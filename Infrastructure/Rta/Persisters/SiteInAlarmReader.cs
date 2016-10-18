using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class SiteInAlarmReader : ISiteInAlarmReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public SiteInAlarmReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IEnumerable<SiteInAlarmModel> Read()
		{
			return _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT SiteId, COUNT(*) AS Count
					FROM ReadModel.AgentState
					WHERE AlarmStartTime <= :now
					AND (IsDeleted != 1
					OR IsDeleted IS NULL)
					GROUP BY SiteId
					")
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteInAlarmModel)))
				.List()
				.Cast<SiteInAlarmModel>();
		}

		public IEnumerable<SiteInAlarmModel> ReadForSkills(Guid[] skillIds)
		{
			return _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT a.SiteId, COUNT(*) AS Count
					FROM ReadModel.AgentState AS a

					INNER JOIN ReadModel.GroupingReadOnly AS g
					ON a.PersonId = g.PersonId					
					WHERE g.GroupId IN (:skillIds)
					AND g.PageId = :skillGroupingPageId

					AND a.AlarmStartTime <= :now
					AND (a.IsDeleted != 1
					OR a.IsDeleted IS NULL)
					GROUP BY a.SiteId
					")
				.SetParameter("now", _now.UtcDateTime())
				.SetParameterList("skillIds", skillIds)
				.SetParameter("skillGroupingPageId", HardcodedSkillGroupingPageId.Get)
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteInAlarmModel)))
				.List()
				.Cast<SiteInAlarmModel>();
		}
	}
}