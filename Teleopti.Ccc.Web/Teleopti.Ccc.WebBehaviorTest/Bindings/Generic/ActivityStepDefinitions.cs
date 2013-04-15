using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ActivityStepDefinitions
	{
		[Given(@"there is an activity with")]
		public void GivenThereIsAnActivity(Table table)
		{
			var activity = table.CreateInstance<ActivityConfigurable>();
			UserFactory.User().Setup(activity);
		}

		[Given(@"there is an activity named '(.*)'")]
		public void GivenThereIsAnActivityNamed(string name)
		{
			var activity = new ActivityConfigurable {Name = name};
			UserFactory.User().Setup(activity);
		}
	}

}