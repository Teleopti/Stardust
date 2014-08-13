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
			var activity = table.CreateInstance<ActivityConfigurable>();
			DataMaker.Data().Apply(activity);
		}

		[Given(@"there is an activity named '(.*)'")]
		public void GivenThereIsAnActivityNamed(string name)
		{
			var activity = new ActivityConfigurable {Name = name};
			DataMaker.Data().Apply(activity);
		}

		[Given(@"there are activities")]
		public void GivenThereAreShiftCategories(Table table)
		{
			var shiftCategories = table.CreateSet<ActivityConfigurable>();
			shiftCategories.ForEach(x => DataMaker.Data().Apply(x));
		}

		[Given(@"there is an activity in business unit '(.*)' named '(.*)'")]
		public void GivenThereIsAnActivityInBusinessUnitNamed(string businessUnitName, string activityName)
		{
			var activity = new ActivityConfigurable { Name = activityName, BusinessUnit = businessUnitName};
			DataMaker.Data().Apply(activity);
		}

	}

}