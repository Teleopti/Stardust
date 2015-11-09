using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderUsingFilters : IDayOffOptimizationPreferenceProvider
	{
		private readonly IDaysOffPreferences _daysOffPreferences;

		public DayOffOptimizationPreferenceProviderUsingFilters(IDaysOffPreferences daysOffPreferences)
		{
			_daysOffPreferences = daysOffPreferences;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			//not impl correctly yet - should handle filters
			return _daysOffPreferences;	
		}
	}
}
