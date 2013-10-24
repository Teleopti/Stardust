using Teleopti.Ccc.TestCommon.TestData.Common;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class PersonAbsenceConfigurable : TestCommon.TestData.Setups.PersonAbsenceConfigurable
	{
		public PersonAbsenceConfigurable()
		{
			Scenario = GlobalDataMaker.Data().Data<CommonScenario>().Scenario.Description.Name;
		}
	}
}
