using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFilters : IBlockPreferenceProvider
	{
		private readonly SchedulingOptions _schedulingOptions;
		private readonly IEnumerable<PlanningGroupSettings> _planningGroupSettings;

		public BlockPreferenceProviderUsingFilters(IEnumerable<PlanningGroupSettings> planningGroupSettings, SchedulingOptions schedulingOptions)
		{
			_schedulingOptions = schedulingOptions;
			_planningGroupSettings = planningGroupSettings.OrderBy(x => x.Default);
		}

		public ExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			var planningGroupSettings = _planningGroupSettings
				.FirstOrDefault(x => x.IsValidForAgent(person, dateOnly));
			if (planningGroupSettings == null)
			{
				planningGroupSettings = PlanningGroupSettings.CreateDefault();
				planningGroupSettings.UpdateWith(_schedulingOptions);
			}
			
			return mapToBlockPreference(planningGroupSettings);
		}

		private ExtraPreferences mapToBlockPreference(PlanningGroupSettings settings)
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