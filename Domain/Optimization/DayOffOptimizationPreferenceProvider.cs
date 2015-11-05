using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffOptimizationPreferenceProvider
	{
		IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly);
	}

	public class DayOffOptimizationPreferenceProvider : IDayOffOptimizationPreferenceProvider
	{
		private readonly IDaysOffPreferences _daysOffPreferences;

		public DayOffOptimizationPreferenceProvider(IDaysOffPreferences daysOffPreferences)
		{
			_daysOffPreferences = daysOffPreferences;
		}

		public IDaysOffPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _daysOffPreferences;
		}
	}
}
