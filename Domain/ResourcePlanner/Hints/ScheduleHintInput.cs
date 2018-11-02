using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class ScheduleHintInput
	{
		public ScheduleHintInput(IEnumerable<IPerson> people, DateOnlyPeriod period, IBlockPreferenceProvider blockPreferenceProvider, bool usePreferences)
		{
			People = people;
			Period = period;
			BlockPreferenceProvider = blockPreferenceProvider;
			UsePreferences = usePreferences;
		}

		public IEnumerable<IPerson> People { get; }
		public DateOnlyPeriod Period { get; }
		public IBlockPreferenceProvider BlockPreferenceProvider { get; }
		public bool UsePreferences { get; }
	}
}