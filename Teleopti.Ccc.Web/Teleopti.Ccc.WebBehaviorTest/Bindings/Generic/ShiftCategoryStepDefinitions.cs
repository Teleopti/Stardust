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

		[Given(@"there are shift categories")]
		public void GivenThereAreShiftCategories(Table table)
		{
			var shiftCategories = table.CreateSet<ShiftCategoryConfigurable>();
			UserFactory.User().Setup(new ShiftCategoryDataSetup(shiftCategories));
		}

	}
}