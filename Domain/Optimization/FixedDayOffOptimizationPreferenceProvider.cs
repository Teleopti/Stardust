using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FixedBlockPreferenceProvider : IBlockPreferenceProvider
	{
		private readonly IExtraPreferences _extraPreferences;

		public FixedBlockPreferenceProvider(IExtraPreferences extraPreferences)
		{
			_extraPreferences = extraPreferences;
		}

		public IExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _extraPreferences;
		}
	}

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
