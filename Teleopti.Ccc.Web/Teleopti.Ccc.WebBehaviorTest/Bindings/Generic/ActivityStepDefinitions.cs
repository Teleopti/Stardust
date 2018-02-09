using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
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
			var activity = table.CreateInstance<ActivitySpec>();
			DataMaker.Data().Apply(activity);
		}

		[Given(@"there is an activity named '(.*)'")]
		public void GivenThereIsAnActivityNamed(string name)
		{
			var activity = new ActivitySpec {Name = name};
			DataMaker.Data().Apply(activity);
		}

		[Given(@"there are activities")]
		public void GivenThereAreShiftCategories(Table table)
		{
			var shiftCategories = table.CreateSet<ActivitySpec>();
			shiftCategories.ForEach(x => DataMaker.Data().Apply(x));
		}
	}

}