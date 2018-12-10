using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class ScheduleHintInput
	{
		public ScheduleHintInput(IEnumerable<IPerson> people, DateOnlyPeriod period, double preferenceValueValue)
		{
			People = people;
			Period = period;
			PreferencesValue = preferenceValueValue;
		}

		public IEnumerable<IPerson> People { get; }
		public DateOnlyPeriod Period { get; }
		public double PreferencesValue { get; }
	}
}