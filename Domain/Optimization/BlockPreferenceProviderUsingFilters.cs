using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class BlockPreferenceProviderUsingFilters : IBlockPreferenceProvider
	{
		private readonly IEnumerable<PlanningGroupSettings> _planningGroupSettings;

		public BlockPreferenceProviderUsingFilters(IEnumerable<PlanningGroupSettings> planningGroupSettings)
		{
			_planningGroupSettings = planningGroupSettings.OrderBy(x => x.Default);
		}

		public IExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			foreach (var settings in _planningGroupSettings.Where(s => s.IsValidForAgent(person, dateOnly)))
			{
				return mapToBlockPreference(settings);
			}

			return mapToBlockPreference(PlanningGroupSettings.CreateDefault());
		}

		private IExtraPreferences mapToBlockPreference(PlanningGroupSettings settings)
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