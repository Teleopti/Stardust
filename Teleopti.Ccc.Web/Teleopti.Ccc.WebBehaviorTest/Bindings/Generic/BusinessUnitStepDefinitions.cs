using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BusinessUnitStepDefinitions
	{
		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnit = table.CreateInstance<BusinessUnitConfigurable>();
			DataMaker.Data().Apply(businessUnit);
		}

	}
}