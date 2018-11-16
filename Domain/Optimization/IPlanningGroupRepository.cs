using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupRepository : IRepository<PlanningGroup>
	{
		PlanningGroup FindPlanningGroupBySettingId(Guid planningGroupSettingId);
	}
}