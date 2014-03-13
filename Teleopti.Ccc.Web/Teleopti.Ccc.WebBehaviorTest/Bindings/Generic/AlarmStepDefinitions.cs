using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class AlarmStepDefinitions
	{
		[Given(@"there is an alarm with")]
		public void GivenThereIsAnAlarmWith(Table table)
		{
			DataMaker.Data().Apply(table.CreateInstance<AlarmConfigurable>());
		}
	}
}