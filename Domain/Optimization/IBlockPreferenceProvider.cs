using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IBlockPreferenceProvider
	{
		ExtraPreferences ForAgent(IPerson person, DateOnly dateOnly);
		IEnumerable<ExtraPreferences> ForAgents(IEnumerable<IPerson> persons, DateOnly dateOnly);
	}
}