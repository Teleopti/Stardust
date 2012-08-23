using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinition
	{
		[Given(@"Current time is '(.*)'")]
		public void GivenCurrentTimeIs(DateTime time)
		{
			Navigation.GoTo("Test/SetCurrentTime?dateSet=" + time);
		}

	}
}