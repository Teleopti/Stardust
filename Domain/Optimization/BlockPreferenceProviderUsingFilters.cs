using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFilters : IBlockPreferenceProvider
	{
		private readonly SchedulingOptions _schedulingOptions;
		private readonly AllPlanningGroupSettings _planningGroupSettings;

		public BlockPreferenceProviderUsingFilters(AllPlanningGroupSettings planningGroupSettings, SchedulingOptions schedulingOptions)
		{
			_schedulingOptions = schedulingOptions;
			_planningGroupSettings = planningGroupSettings;
		}

		public ExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			var planningGroupSettings = _planningGroupSettings.ForAgent(person, dateOnly);
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