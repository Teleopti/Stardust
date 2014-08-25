﻿using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Legacy.Common;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class ShiftConfigurable : TestCommon.TestData.Setups.Configurable.ShiftConfigurable
	{
		public ShiftConfigurable()
		{
			Scenario = GlobalDataMaker.Data().Data<DefaultScenario>().Scenario.Description.Name;
		}
	}
}