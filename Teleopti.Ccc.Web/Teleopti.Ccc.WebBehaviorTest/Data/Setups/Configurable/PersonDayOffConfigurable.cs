using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonDayOffConfigurable : TestCommon.TestData.Setups.Configurable.PersonDayOffConfigurable
	{
		public PersonDayOffConfigurable()
		{
			Scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario.Description.Name;
		}
	}
}
