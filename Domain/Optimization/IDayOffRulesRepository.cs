using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesRepository : IRepository<DayOffRules>
	{
		IList<DayOffRules> LoadAllByPlanningGroup(IPlanningGroup planningGroup);
		IList<DayOffRules> LoadAllWithoutPlanningGroup();
		void RemoveForPlanningGroup(IPlanningGroup planningGroup);
	}
}