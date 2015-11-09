using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffOptimizationPreferenceProvider
	{
		IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly);
	}
}