using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FixedDayOffOptimizationPreferenceProvider : IDayOffOptimizationPreferenceProvider
	{
		private readonly IDaysOffPreferences _daysOffPreferences;

		public FixedDayOffOptimizationPreferenceProvider(IDaysOffPreferences daysOffPreferences)
		{
			_daysOffPreferences = daysOffPreferences;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _daysOffPreferences;
		}
	}
}
