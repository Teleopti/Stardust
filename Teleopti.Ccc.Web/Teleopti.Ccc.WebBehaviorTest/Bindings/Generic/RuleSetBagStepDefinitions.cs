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
		public static RuleSetBagConfigurable RuleSetBagConfigurableTransform(Table table)
		{
			return table.CreateInstance<RuleSetBagConfigurable>();
		}

		[StepArgumentTransformation]
		public static WorkShiftRuleSetConfigurable WorkShiftRuleSetConfigurableTransform(Table table)
		{
			return table.CreateInstance<WorkShiftRuleSetConfigurable>();
		}

		[Given(@"there is a rule set with")]
		public void GivenThereIsARuleSetWith(WorkShiftRuleSetConfigurable workShiftRuleSet)
		{
			DataMaker.Data().Apply(workShiftRuleSet);
		}

		[Given(@"there is a shift bag with")]
		public void GivenThereIsARuleSetBagWith(RuleSetBagConfigurable ruleSetBag)
		{
			DataMaker.Data().Apply(ruleSetBag);
		}

	}
}