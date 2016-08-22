using System.Collections.Generic;
using System.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Rta.Persisters
{
	public class SiteInAlarmFromAgentStatesReadModelReader : ISiteOutOfAdherenceReadModelReader
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;
		private readonly INow _now;

		public SiteInAlarmFromAgentStatesReadModelReader(ICurrentUnitOfWork currentUnitOfWork, INow now)
		{
			_currentUnitOfWork = currentUnitOfWork;
			_now = now;
		}

		public IEnumerable<SiteOutOfAdherenceReadModel> Read()
		{
			return _currentUnitOfWork.Current().Session()
				.CreateSQLQuery(@"
					SELECT SiteId, COUNT(*) AS Count
					FROM ReadModel.AgentState
					WHERE AlarmStartTime <= :now
					GROUP BY SiteId
					")
				.SetParameter("now", _now.UtcDateTime())
				.SetResultTransformer(Transformers.AliasToBean(typeof(SiteOutOfAdherenceReadModel)))
				.List()
				.Cast<SiteOutOfAdherenceReadModel>();
		}
	}
}