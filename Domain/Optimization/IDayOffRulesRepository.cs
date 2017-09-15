using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesRepository : IRepository<PlanningGroupSettings>
	{
		IList<PlanningGroupSettings> LoadAllByPlanningGroup(IPlanningGroup planningGroup);
		IList<PlanningGroupSettings> LoadAllWithoutPlanningGroup();
		void RemoveForPlanningGroup(IPlanningGroup planningGroup);
	}
}