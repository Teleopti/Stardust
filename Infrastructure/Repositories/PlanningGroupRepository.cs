using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
	public class PlanningGroupRepository : Repository<PlanningGroup>, IPlanningGroupRepository
	{
		public PlanningGroupRepository(ICurrentUnitOfWork currentUnitOfWork) : base(currentUnitOfWork)
		{
		}
		
		public PlanningGroup FindPlanningGroupBySettingId(Guid planningGroupSettingId)
		{
			return Session.GetNamedQuery("FindPlanningGroupBySettingId")
				.SetGuid("settingId", planningGroupSettingId)
				.UniqueResult<PlanningGroup>();
		}
	}
}