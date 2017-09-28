using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public class BlockPreferencesMapper
	{
		public void UpdateSchedulingOptionsFromExtraPreferences(SchedulingOptions schedulingOptions, IExtraPreferences[] blockPreferences)
		{
			var distinctTypes = blockPreferences.Select(x => x.BlockTypeValue).Distinct().ToArray();
			var blockFinderType =
				distinctTypes.Length == 1
					? distinctTypes.Single()
					: blockPreferences.First(x => x.BlockTypeValue != BlockFinderType.SingleDay).BlockTypeValue;
			schedulingOptions.UseBlock = blockPreferences.Any(x => x.UseTeamBlockOption);
			schedulingOptions.BlockFinderTypeForAdvanceScheduling = schedulingOptions.UseBlock
				? blockFinderType
				: BlockFinderType.SingleDay;
			schedulingOptions.BlockSameShift = blockPreferences.Any(x => x.UseBlockSameShift);
			schedulingOptions.BlockSameShiftCategory = blockPreferences.Any(x => x.UseBlockSameShiftCategory);
			schedulingOptions.BlockSameStartTime = blockPreferences.Any(x => x.UseBlockSameStartTime);
		}

		public void UpdateSchedulingOptionsFromOptimizationPreferences(SchedulingOptions schedulingOptions, IOptimizationPreferences optimizationPreferences)
		{
			schedulingOptions.BlockFinderTypeForAdvanceScheduling = optimizationPreferences.Extra.BlockTypeValue;
			schedulingOptions.UseSameDayOffs = optimizationPreferences.Extra.UseTeamBlockOption || optimizationPreferences.Extra.UseTeamSameDaysOff;

			schedulingOptions.UseBlock = optimizationPreferences.Extra.UseTeamBlockOption;
			schedulingOptions.BlockSameEndTime = optimizationPreferences.Extra.UseBlockSameEndTime;
			schedulingOptions.BlockSameStartTime = optimizationPreferences.Extra.UseBlockSameStartTime;
			schedulingOptions.BlockSameShift = optimizationPreferences.Extra.UseBlockSameShift;
			schedulingOptions.BlockSameShiftCategory = optimizationPreferences.Extra.UseBlockSameShiftCategory;

			if (!optimizationPreferences.Extra.UseTeams)
				schedulingOptions.GroupOnGroupPageForTeamBlockPer = GroupPageLight.SingleAgentGroup(Resources.SingleAgentTeam);

			if (!optimizationPreferences.Extra.UseTeamBlockOption)
				schedulingOptions.BlockFinderTypeForAdvanceScheduling = BlockFinderType.SingleDay;

		}

		public void UpdateOptimizationPreferenceFromSchedulingOptions(IOptimizationPreferences optimizationPreferences, SchedulingOptions schedulingOptions)
		{
			optimizationPreferences.Extra.BlockTypeValue = schedulingOptions.BlockFinderTypeForAdvanceScheduling;
			optimizationPreferences.Extra.UseTeamBlockOption = schedulingOptions.UseBlock;
			optimizationPreferences.Extra.UseBlockSameShift = schedulingOptions.BlockSameShift;
			optimizationPreferences.Extra.UseBlockSameShiftCategory = schedulingOptions.BlockSameShiftCategory;
			optimizationPreferences.Extra.UseBlockSameStartTime = schedulingOptions.BlockSameStartTime;
		}

		public void UpdateOptimizationPreferencesFromExtraPreferences(IOptimizationPreferences optimizationPreferences, IExtraPreferences blockPreference)
		{
			optimizationPreferences.Extra.BlockTypeValue = blockPreference.BlockTypeValue;
			optimizationPreferences.Extra.UseBlockSameShiftCategory = blockPreference.UseBlockSameShiftCategory;
			optimizationPreferences.Extra.UseBlockSameShift = blockPreference.UseBlockSameShift;
			optimizationPreferences.Extra.UseBlockSameStartTime = blockPreference.UseBlockSameStartTime;
			optimizationPreferences.Extra.UseTeamBlockOption = blockPreference.UseTeamBlockOption;
		}
	}
}