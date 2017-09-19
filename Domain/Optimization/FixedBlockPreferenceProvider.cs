using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public class FixedBlockPreferenceProvider : IBlockPreferenceProvider
	{
		private readonly IExtraPreferences _extraPreferences;

		public FixedBlockPreferenceProvider(IExtraPreferences extraPreferences)
		{
			_extraPreferences = new ExtraPreferences
			{
				BlockTypeValue = extraPreferences.BlockTypeValue,
				UseBlockSameEndTime = extraPreferences.UseBlockSameEndTime,
				UseBlockSameShiftCategory = extraPreferences.UseBlockSameShiftCategory,
				UseBlockSameStartTime = extraPreferences.UseBlockSameStartTime,
				UseBlockSameShift = extraPreferences.UseBlockSameShift,
				UseTeamBlockOption = extraPreferences.UseTeamBlockOption
			};
		}

		public IExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _extraPreferences;
		}
	}
}