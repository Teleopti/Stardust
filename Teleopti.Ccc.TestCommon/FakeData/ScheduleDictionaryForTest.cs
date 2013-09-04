using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class ScheduleDictionaryForTest : ScheduleDictionary
	{
		public ScheduleDictionaryForTest(IScenario scenario, IScheduleDateTimePeriod period, IDictionary<IPerson, IScheduleRange> dictionary)
			: base(scenario, period, dictionary) { }

		public ScheduleDictionaryForTest(IScenario scenario, DateTimePeriod period)
			: base(scenario, new ScheduleDateTimePeriod(period), new Dictionary<IPerson, IScheduleRange>()) { }

		public void AddTestItem(IPerson person, IScheduleRange range)
		{
			BaseDictionary.Add(person, range);
		}
	}
}
