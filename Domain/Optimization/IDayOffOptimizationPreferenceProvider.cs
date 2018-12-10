using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffOptimizationPreferenceProvider
	{
		IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly);
	}
}