using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupGlobalSettingSetter
	{
		public void Execute(PlanningGroup planningGroup, IOptimizationPreferences optimizationPreferences)
		{
			if (planningGroup != null)
			{
				optimizationPreferences.General.UsePreferences = planningGroup.PreferenceValue > Percent.Zero;
				optimizationPreferences.General.PreferencesValue = planningGroup.PreferenceValue.Value;
			}
		}
	}
}