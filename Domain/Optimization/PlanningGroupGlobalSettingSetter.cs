using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class PlanningGroupGlobalSettingSetterOld:IPlanningGroupGlobalSettingSetter
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
	
	public class PlanningGroupGlobalSettingSetter:IPlanningGroupGlobalSettingSetter
	{
		public void Execute(AllSettingsForPlanningGroup allSettingsForPlanningGroup, IOptimizationPreferences optimizationPreferences)
		{
			if (allSettingsForPlanningGroup == null) 
				return;
			optimizationPreferences.General.UsePreferences = allSettingsForPlanningGroup.PreferenceValue > Percent.Zero;
			optimizationPreferences.General.PreferencesValue = allSettingsForPlanningGroup.PreferenceValue.Value;

			if (allSettingsForPlanningGroup.TeamSettings.GroupPageType == GroupPageType.SingleAgent) 
				return;
			optimizationPreferences.Extra.UseTeams = true;
			optimizationPreferences.Extra.TeamGroupPage = new GroupPageLight("_",allSettingsForPlanningGroup.TeamSettings.GroupPageType);
			
			switch (allSettingsForPlanningGroup.TeamSettings.TeamSameType)
			{
				case TeamSameType.StartTime:
					optimizationPreferences.Extra.UseTeamSameStartTime = true;
					break;
				case TeamSameType.EndTime:
					optimizationPreferences.Extra.UseTeamSameEndTime = true;
					break;
				case TeamSameType.ShiftCategory:
					optimizationPreferences.Extra.UseTeamSameShiftCategory = true;
					break;
			}
		}
	}

	public interface IPlanningGroupGlobalSettingSetter
	{
		void Execute(AllSettingsForPlanningGroup allSettingsForPlanningGroup,
			IOptimizationPreferences optimizationPreferences);
	}
}