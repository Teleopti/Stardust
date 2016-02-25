using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonDayOffConfigurable : TestCommon.TestData.Setups.Configurable.PersonDayOffConfigurable
	{
		public PersonDayOffConfigurable()
		{
			Scenario = DefaultScenario.Scenario.Description.Name;
		}
	}
}
