using System;
using System.Linq;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningGroupRepository :  Repository<PlanningGroup>, IPlanningGroupRepository
	{
		public PlanningGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
		
		//To be continued
		public PlanningGroup FindPlanningGroupBySettingId(Guid planningGroupSettingId)
		{
			return Session.CreateCriteria<PlanningGroup>()
				.SetFetchMode("Settings", FetchMode.Join)
				.SetResultTransformer(Transformers.DistinctRootEntity)
				.List<PlanningGroup>().Single();
		}
	}
}