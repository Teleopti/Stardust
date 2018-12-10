using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class SchedulePostHintInput : ScheduleHintInput
	{
		public IScheduleDictionary Schedules { get; }
		public IBlockPreferenceProvider BlockPreferenceProvider { get; }

		public SchedulePostHintInput(IScheduleDictionary schedules, IEnumerable<IPerson> people, DateOnlyPeriod period,
			IBlockPreferenceProvider blockPreferenceProvider, double preferenceValue) : base(people, period, preferenceValue)
		{
			Schedules = schedules;
			BlockPreferenceProvider = blockPreferenceProvider;
		}
	}
}