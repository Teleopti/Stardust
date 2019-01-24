using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class UpdateSchedulingOptionsWithTeamSettings:IUpdateSchedulingOptionsWithTeamSettings
	{
		public void Execute(SchedulingOptions schedulingOptions, AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			if (allSettingsForPlanningGroup.TeamSettings.GroupPageType != GroupPageType.SingleAgent)
			{
				schedulingOptions.UseTeam = true;
				schedulingOptions.GroupOnGroupPageForTeamBlockPer = new GroupPageLight(UserTexts.Resources.Main, allSettingsForPlanningGroup.TeamSettings.GroupPageType);
				if (allSettingsForPlanningGroup.TeamSettings.TeamSameType == TeamSameType.ShiftCategory)
				{
					schedulingOptions.TeamSameShiftCategory = true;
				}else if (allSettingsForPlanningGroup.TeamSettings.TeamSameType == TeamSameType.StartTime)
				{
					schedulingOptions.TeamSameStartTime = true;
				}
			}
		}
	}
	public class NoUpdateForDesktop:IUpdateSchedulingOptionsWithTeamSettings
	{
		public void Execute(SchedulingOptions schedulingOptions, AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
		}
	}

	public interface IUpdateSchedulingOptionsWithTeamSettings
	{
		void Execute(SchedulingOptions schedulingOptions, AllSettingsForPlanningGroup allSettingsForPlanningGroup);
	}
}