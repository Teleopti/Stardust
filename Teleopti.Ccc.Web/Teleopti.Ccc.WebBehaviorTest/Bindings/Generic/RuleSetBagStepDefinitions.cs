using System.Globalization;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class RuleSetBagStepDefinitions
	{
		[StepArgumentTransformation]
		public RuleSetBagConfigurable WorkShiftRuleSetConfigurableTransform(Table table)
		{
			return table.CreateInstance<RuleSetBagConfigurable>();
		}

		[Given(@"there is a rule set bag with")]
		public void GivenThereIsARuleSetBagWith(RuleSetBagConfigurable ruleSetBag)
		{
			UserFactory.User().Setup(ruleSetBag);
		}
	}
}