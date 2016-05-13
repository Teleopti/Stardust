using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
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

			var businessUnitAnalytics = new BusinessUnit(businessUnitApp.BusinessUnit, DefaultAnalyticsDataCreator.GetDataSources(), ++BusinessUnit.IdCounter);
			DataMaker.Data().Analytics().Apply(businessUnitAnalytics);
		}

		[Given(@"there is a business unit with")]
		public void GivenThereIsABusinessUnitWith(Table table)
		{
			var businessUnitApp = table.CreateInstance<BusinessUnitConfigurable>();
			DataMaker.Data().Apply(businessUnitApp);

			var businessUnitAnalytics = new BusinessUnit(businessUnitApp.BusinessUnit, DefaultAnalyticsDataCreator.GetDataSources(), ++BusinessUnit.IdCounter);
			DataMaker.Data().Analytics().Apply(businessUnitAnalytics);
		}

	}
}