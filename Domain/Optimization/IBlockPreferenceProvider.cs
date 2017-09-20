using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IBlockPreferenceProvider
	{
		IExtraPreferences ForAgent(IPerson person, DateOnly dateOnly);
		IEnumerable<IExtraPreferences> ForAgents(IEnumerable<IPerson> persons, DateOnly dateOnly);
	}
}