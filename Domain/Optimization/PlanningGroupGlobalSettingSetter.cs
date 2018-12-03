using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupGlobalSettingSetter
	{
		public void Execute(AllSettingsForPlanningGroup allSettingsForPlanningGroup, IOptimizationPreferences optimizationPreferences)
		{
			if (allSettingsForPlanningGroup != null)
			{
				optimizationPreferences.General.UsePreferences = allSettingsForPlanningGroup.PreferenceValue > Percent.Zero;
				optimizationPreferences.General.PreferencesValue = allSettingsForPlanningGroup.PreferenceValue.Value;
			}
		}
	}
}