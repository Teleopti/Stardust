using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftCategoryStepDefinitions
	{
		[Given(@"there is a shift category with")]
		public void GivenThereIsAShiftCategory(Table table)
		{
			var shiftCategory = table.CreateInstance<ShiftCategoryConfigurable>();
			UserFactory.User().Setup(shiftCategory);
		}

		[Given(@"there is a shift category named '(.*)'")]
		public void GivenThereIsAShiftCategoryNamed(string name)
		{
			var shiftCategory = new ShiftCategoryConfigurable {Name = name};
			UserFactory.User().Setup(shiftCategory);
		}
	}
}