using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings
{
	[Binding]
	public class ConfigurableUserDataStepDefinitions
	{
		[Given(@"I have a preference with")]
		public void GivenIHaveAPreferenceWith(Table table)
		{
			var preference = table.CreateInstance<PreferenceConfigurable>();
			UserFactory.User().Setup(preference);
		}

		[Given(@"I have an extended preference on '(.*)'")]
		public void GivenIHaveAnExtendedPreferenceOn(DateTime date)
		{
			UserFactory.User().Setup(new PreferenceConfigurable { Date = date, IsExtended = true });
		}
	}
}