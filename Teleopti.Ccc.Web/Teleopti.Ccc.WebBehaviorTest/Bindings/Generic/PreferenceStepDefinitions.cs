using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PreferenceStepDefinitions
	{
		[Given(@"I have a preference with")]
		public void GivenIHaveAPreferenceWith(Table table)
		{
			var preference = table.CreateInstance<PreferenceConfigurable>();
			DataMaker.Data().Apply(preference);
		}

		[Given(@"I have an extended preference with")]
		public void GivenIHaveAnExtendedPreferenceWith(Table table)
		{
			var preference = table.CreateInstance<PreferenceConfigurable>();
			preference.IsExtended = true;
			DataMaker.Data().Apply(preference);
		}

		[Given(@"I have an extended preference on '(.*)'")]
		public void GivenIHaveAnExtendedPreferenceOn(DateTime date)
		{
			DataMaker.Data().Apply(new PreferenceConfigurable { Date = date, IsExtended = true });
		}

	}
}