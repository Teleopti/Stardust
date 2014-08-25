using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonAbsenceConfigurable : TestCommon.TestData.Setups.Configurable.PersonAbsenceConfigurable
	{
		public PersonAbsenceConfigurable()
		{
			Scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario.Description.Name;
		}
	}
}
