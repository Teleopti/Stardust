﻿using Teleopti.Ccc.TestCommon.TestData.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class ShiftConfigurable : TestCommon.TestData.Setups.Configurable.ShiftConfigurable
	{
		public ShiftConfigurable()
		{
			Scenario = DefaultScenario.Scenario.Description.Name;
		}
	}
}