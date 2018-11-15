using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupSettingsRepository : IRepository<PlanningGroupSettings>
	{
		AllPlanningGroupSettings LoadAllByPlanningGroup(IPlanningGroup planningGroup);
		void RemoveForPlanningGroup(IPlanningGroup planningGroup);
	}
}