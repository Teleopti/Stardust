using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class DayOffOptimizationPreferenceProviderWeb : IDayOffOptimizationPreferenceProvider
	{
		private readonly IDaysOffPreferences _daysOffPreferences;

		public DayOffOptimizationPreferenceProviderWeb(IDaysOffPreferences daysOffPreferences)
		{
			_daysOffPreferences = daysOffPreferences;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _daysOffPreferences;	
		}
	}
}
