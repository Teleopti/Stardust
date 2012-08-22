using System;
using TechTalk.SpecFlow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TimeStepDefinition
	{
		[Given(@"Current time is '(.*)'")]
		public void GivenCurrentTimeIs(DateTime time)
		{

			ScenarioContext.Current.Pending();
		}

	}
}