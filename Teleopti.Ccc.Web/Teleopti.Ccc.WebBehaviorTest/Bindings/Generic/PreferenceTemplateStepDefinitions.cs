using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class PreferenceTemplateStepDefinitions
	{

		[Given(@"I have a preference template with")]
		public void GivenIHaveAPreferenceTemplateWith(Table table)
		{
			var preferenceTemplate = table.CreateInstance<PreferenceTemplateConfigurable>();
			DataMaker.Data().Apply(preferenceTemplate);
		}

	}
}