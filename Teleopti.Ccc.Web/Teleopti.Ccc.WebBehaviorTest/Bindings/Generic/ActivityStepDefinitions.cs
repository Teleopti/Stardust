using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ActivityStepDefinitions
	{
		[Given(@"there is an activity with")]
		public void GivenThereIsAnActivity(Table table)
		{
			var activity = table.CreateInstance<ActivityConfigurable>();
			DataMaker.Data().Apply(activity);
		}

		[Given(@"there is an activity named '(.*)'")]
		public void GivenThereIsAnActivityNamed(string name)
		{
			var activity = new ActivityConfigurable {Name = name};
			DataMaker.Data().Apply(activity);
		}
	}

}