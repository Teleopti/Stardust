using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonAbsenceConfigurable : TestCommon.TestData.Setups.Configurable.PersonAbsenceConfigurable
	{
		public PersonAbsenceConfigurable()
		{
			Scenario = DefaultScenario.Scenario.Description.Name;
		}
	}
}
