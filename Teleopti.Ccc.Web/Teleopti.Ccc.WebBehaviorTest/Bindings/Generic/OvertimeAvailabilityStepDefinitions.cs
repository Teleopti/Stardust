using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class OvertimeAvailabilityStepDefinitions
	{
		[Given(@"I have an overtime availability with")]
		public void GivenIHaveAnOvertimeAvailabilityWith(Table table)
		{
			var fields = table.CreateInstance<OvertimeAvailabilityConfigurable>();
			UserFactory.User().Setup(fields);
		}
	}

	public class OvertimeAvailabilityFields
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public string StartTime { get; set; }
		public string EndTime { get; set; }
		public bool EndTimeNextDay { get; set; }
	}

	public class OvertimeAvailabilityTooltip
	{
		public string StartTime { get; set; }
		public string EndTime { get; set; }
	}
}