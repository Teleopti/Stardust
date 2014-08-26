using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;

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
