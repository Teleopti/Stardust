using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class SchedulePostHintInput : ScheduleHintInput
	{
		public IScheduleDictionary Schedules { get; }

		public SchedulePostHintInput(IScheduleDictionary schedules, IEnumerable<IPerson> people, DateOnlyPeriod period,
			IBlockPreferenceProvider blockPreferenceProvider, bool usePreferences) : base(people, period,
			blockPreferenceProvider, usePreferences)
		{
			Schedules = schedules;
		}
	}
}