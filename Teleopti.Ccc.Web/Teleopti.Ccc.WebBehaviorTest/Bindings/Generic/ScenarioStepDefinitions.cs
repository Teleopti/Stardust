using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ScenarioStepDefinitions
	{
		[Given(@"there is a scenario")]
		public void GivenThereIsAScenario(Table table)
		{
			var scenario = table.CreateInstance<ScenarioConfigurable>();
			DataMaker.Data().Apply(scenario);
		}
	}
}