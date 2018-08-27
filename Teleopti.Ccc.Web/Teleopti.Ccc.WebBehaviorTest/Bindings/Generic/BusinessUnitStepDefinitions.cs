using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class BusinessUnitStepDefinitions
	{
		

		[Given(@"there is a business unit named '(.*)'")]
		public void GivenThereIsABusinessUnitWith(string name)
		{
			var businessUnitApp = new BusinessUnitConfigurable { Name = name };
			DataMaker.Data().Apply(businessUnitApp);
		}

		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnitApp = table.CreateInstance<BusinessUnitConfigurable>();
			DataMaker.Data().Apply(businessUnitApp);
		}

		[Given(@"the business unit scope is using '(.*)'")]
		public void GivenTheBusinessUnitScopeIsUsing(string name)
		{
			var businessUnitConfigurable = DataMaker.Data().UserDatasOfType<BusinessUnitConfigurable>().First(x => x.Name == name);
			CurrentScopeBusinessUnit.Use(businessUnitConfigurable);
		}
	}
}