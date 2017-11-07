using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class HintInput
	{
		public IScheduleDictionary Schedules { get; set; }
		public IEnumerable<IPerson> People { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IBlockPreferenceProvider BlockPreferenceProvider { get; set; }
		public IScheduleDictionary CurrentSchedule { get; set; }

		public HintInput(IScheduleDictionary schedules, IEnumerable<IPerson> people, DateOnlyPeriod period, IBlockPreferenceProvider blockPreferenceProvider)
		{
			Schedules = schedules;
			People = people;
			Period = period;
			BlockPreferenceProvider = blockPreferenceProvider;
		}
	}
}