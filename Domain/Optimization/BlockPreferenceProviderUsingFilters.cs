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
			return mapToBlockPreference(_planningGroupSettings.Where(planningGroupSettings => planningGroupSettings.IsValidForAgent(person, dateOnly)).OrderByDescending(x => x.Priority).FirstOrDefault() ?? PlanningGroupSettings.CreateDefault());
		}
		
		public IEnumerable<IExtraPreferences> ForAgents(IEnumerable<IPerson> persons, DateOnly dateOnly)
		{
			return persons.Select(person => ForAgent(person, dateOnly)).ToList();
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