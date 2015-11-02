using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FixedOptimizationPreferencesProvider : IOptimizationPreferencesProvider
	{
		private readonly OptimizationPreferences _optimizationPreferences;

		public FixedOptimizationPreferencesProvider(OptimizationPreferences optimizationPreferences)
		{
			_optimizationPreferences = optimizationPreferences;
		}

		public OptimizationPreferences ForAgent(IPerson agent, DateOnly date)
		{
			return _optimizationPreferences;
		}
	}
}