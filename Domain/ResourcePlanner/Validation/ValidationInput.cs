using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Validation
{
	public class ValidationInput
	{
		public IScheduleDictionary Schedules { get; set; }
		public IEnumerable<IPerson> People { get; set; }
		public DateOnlyPeriod Period { get; set; }
		public IBlockPreferenceProvider BlockPreferenceProvider { get; set; }
		public IScheduleDictionary CurrentSchedule { get; set; }

		public ValidationInput(IScheduleDictionary schedules, IEnumerable<IPerson> people, DateOnlyPeriod period)
		{
			Schedules = schedules;
			People = people;
			Period = period;
		}
	}
}