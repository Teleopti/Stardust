using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class ScheduleHintInput
	{
		public ScheduleHintInput(IEnumerable<IPerson> people, DateOnlyPeriod period, bool usePreferences)
		{
			People = people;
			Period = period;
			UsePreferences = usePreferences;
		}

		public IEnumerable<IPerson> People { get; }
		public DateOnlyPeriod Period { get; }
		public bool UsePreferences { get; }
	}
}