using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IPlanningGroupSettingsRepository : IRepository<PlanningGroupSettings>
	{
		AllPlanningGroupSettings LoadAllByPlanningGroup(PlanningGroup planningGroup);
		void RemoveForPlanningGroup(PlanningGroup planningGroup);
	}
}