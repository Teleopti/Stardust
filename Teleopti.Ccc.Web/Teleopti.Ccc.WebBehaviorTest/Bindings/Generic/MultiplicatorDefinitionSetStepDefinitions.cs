using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class MultiplicatorDefinitionSetStepDefinitions
	{
		[Given(@"there is a multiplicator definition set named ""(.*)""")]
		public void GivenThereIsAMultiplicatorDefinitionSetNamed(string name)
		{
			var multiplicatorDefinitionSet = new MultiplicatorDefinitionSetConfigurable { Name = name };
			DataMaker.Data().Apply(multiplicatorDefinitionSet);
		}

	}
}