using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupGlobalSettingSetter
	{
		public void Execute(AllSettingsForPlanningGroup allSettingsForPlanningGroup, IOptimizationPreferences optimizationPreferences)
		{
			if (allSettingsForPlanningGroup.PreferenceValue.HasValue)
			{
				optimizationPreferences.General.UsePreferences = true;
				optimizationPreferences.General.PreferencesValue = allSettingsForPlanningGroup.PreferenceValue.Value.Value;
			}
			else
			{
				optimizationPreferences.General.UsePreferences = false;
			}
		}
	}
}