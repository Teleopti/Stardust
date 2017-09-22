using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
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

		public FixedBlockPreferenceProvider(SchedulingOptions schedulingOptions)
		{
			_extraPreferences = new ExtraPreferences
			{
				BlockTypeValue = schedulingOptions.BlockFinderTypeForAdvanceScheduling,
				UseBlockSameEndTime = schedulingOptions.BlockSameEndTime,
				UseBlockSameShiftCategory = schedulingOptions.BlockSameShiftCategory,
				UseBlockSameStartTime = schedulingOptions.BlockSameStartTime,
				UseBlockSameShift = schedulingOptions.BlockSameShift,
				UseTeamBlockOption = schedulingOptions.UseBlock
			};
		}

		public IExtraPreferences ForAgent(IPerson person, DateOnly dateOnly)
		{
			return _extraPreferences;
		}

		public IEnumerable<IExtraPreferences> ForAgents(IEnumerable<IPerson> persons, DateOnly dateOnly)
		{
			return new List<IExtraPreferences> {_extraPreferences};
		}
	}
}