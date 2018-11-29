using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public static class BlockPreferencesExtensions
	{
		public static IEnumerable<ExtraPreferences> ForAgents(this IBlockPreferenceProvider blockPreferenceProvider,
			IEnumerable<IPerson> persons, DateOnly dateOnly)
		{
			return new HashSet<ExtraPreferences>(persons.Select(person => blockPreferenceProvider.ForAgent(person, dateOnly)));
		}
	}
}