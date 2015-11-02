using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IOptimizationPreferencesProvider
	{
		OptimizationPreferences ForAgent(IPerson agent, DateOnly date);
	}
}