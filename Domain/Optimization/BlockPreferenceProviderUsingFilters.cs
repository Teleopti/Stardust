using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFilters : IBlockPreferenceProvider
	{
		private readonly AllSettingsForPlanningGroup _allSettingsForPlanningGroup;

		public BlockPreferenceProviderUsingFilters(AllSettingsForPlanningGroup allSettingsForPlanningGroup)
		{
			_allSettingsForPlanningGroup = allSettingsForPlanningGroup;
		}

		public ExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			var planningGroupSettings = _allSettingsForPlanningGroup.ForAgent(person, dateOnly);
			return mapToBlockPreference(planningGroupSettings);
		}

		private static ExtraPreferences mapToBlockPreference(PlanningGroupSettings settings)
		{
			return new ExtraPreferences
			{
				UseBlockSameShiftCategory = settings.BlockSameShiftCategory,
				UseBlockSameShift = settings.BlockSameShift,
				UseBlockSameStartTime = settings.BlockSameStartTime,
				BlockTypeValue = settings.BlockFinderType,
				UseTeamBlockOption = settings.BlockSameShiftCategory || settings.BlockSameShift || settings.BlockSameStartTime
			};
		}
	}
}