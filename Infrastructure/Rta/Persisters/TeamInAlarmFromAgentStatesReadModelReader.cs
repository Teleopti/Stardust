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
	public class TeamInAlarmFromAgentStatesReadModelReader : ITeamInAlarmReader
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly INow _now;

		public TeamInAlarmFromAgentStatesReadModelReader(ICurrentUnitOfWork unitOfWork, INow now)
		{
			_unitOfWork = unitOfWork;
			_now = now;
		}

		public IEnumerable<TeamInAlarmModel> Read(Guid siteId)
		{
			return _unitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT TeamId, COUNT(*) AS Count
					FROM ReadModel.AgentState
					WHERE AlarmStartTime <= :now
					AND SiteId = :siteId
					GROUP BY TeamId
					")
				.SetParameter("siteId", siteId)
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(TeamInAlarmModel)))
				.List()
				.Cast<TeamInAlarmModel>();
		}
	}
}