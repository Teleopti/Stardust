using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeScheduleParameters : IScheduleParameters
	{
		public DateTimePeriod Period { get; private set; }
		public IPerson Person { get; private set; }
		public IScenario Scenario { get; private set; }

		public FakeScheduleParameters()
		{
			Person = PersonFactory.CreatePerson("");
			Scenario = ScenarioFactory.CreateScenario(" c",true,false);
			Period = new DateTimePeriod(DateTime.UtcNow,DateTime.UtcNow);
		}
	}
}