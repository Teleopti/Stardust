using System.Collections.Generic;
using NHibernate;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class StateGroupActivityAlarmRepository : Repository<IStateGroupActivityAlarm>,
		IStateGroupActivityAlarmRepository
	{
		public StateGroupActivityAlarmRepository(IUnitOfWork unitOfWork)
			: base(unitOfWork)
		{
		}

		public StateGroupActivityAlarmRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}

		public override bool ValidateUserLoggedOn
		{
			get { return false; }
		}

		public IList<IStateGroupActivityAlarm> LoadAllCompleteGraph()
		{
			return Session.CreateCriteria(typeof (StateGroupActivityAlarm))
				.SetFetchMode("Activity", FetchMode.Join)
				.SetFetchMode("StateGroup", FetchMode.Join)
				.SetFetchMode("AlarmType", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<IStateGroupActivityAlarm>();
		}
	}
}
