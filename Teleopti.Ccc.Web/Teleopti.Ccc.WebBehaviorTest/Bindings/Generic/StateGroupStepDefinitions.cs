using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class StateGroupStepDefinitions
	{
		[Given(@"there is a state with name '(.*)'")]
		public void GivenThereIsAStateWithNamePhone(string name)
		{
			DataMaker.Data().Apply(new StateGroupConfigurable{ Name = name, PhoneState = name});
		}
	}
}