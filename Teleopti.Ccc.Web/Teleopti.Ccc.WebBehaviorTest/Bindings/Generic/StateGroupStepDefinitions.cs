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

		[Given(@"there are (.*) rta state codes and state code groups")]
		public void GivenThereAreRtaStateCodesAndStateCodeGroups(int count)
		{
			for (var i = 0; i < count; i++)
			{
				var state = "State" + i;
				DataMaker.Data().Apply(new StateGroupConfigurable {Name = state, PhoneState = state});
			}
		}
	}
}