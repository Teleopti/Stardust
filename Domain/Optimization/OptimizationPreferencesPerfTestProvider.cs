using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;

namespace Teleopti.Ccc.Domain.Optimization
{
	[RemoveMeWithToggle("Remove when web supports team/block out of the box", Toggles.ResourcePlanner_RunPerfTestAsTeam_43537)]
	public class OptimizationPreferencesPerfTestProvider : IOptimizationPreferencesProvider
	{
		public IOptimizationPreferences Fetch()
		{
			return new OptimizationPreferences
			{
				General = new GeneralPreferences { ScheduleTag = NullScheduleTag.Instance, OptimizationStepDaysOff = true },
				Extra = new ExtraPreferences { UseTeams = true, UseTeamSameShiftCategory = true, TeamGroupPage = new GroupPageLight(UserTexts.Resources.Main, GroupPageType.Hierarchy) }
			};
		}
	}
}