﻿using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable
{
	public class PersonEmptyDayConfigurable : TestCommon.TestData.Setups.Configurable.PersonEmptyDayConfigurable
	{
		public PersonEmptyDayConfigurable()
		{
			Scenario = DefaultScenario.Scenario.Description.Name;
		}
	}
}
