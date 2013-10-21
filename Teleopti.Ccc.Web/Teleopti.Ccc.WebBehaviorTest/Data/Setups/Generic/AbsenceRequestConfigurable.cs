using Teleopti.Ccc.TestCommon.TestData.Setups;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic
{
	public class AbsenceRequestConfigurable : TestCommon.TestData.Setups.AbsenceRequestConfigurable
	{
		public AbsenceRequestConfigurable()
		{
			Absence = TestData.Absence.Name;
		}
	}
}