using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

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

		[Given(@"there is a shift bag named '(.*)' with rule set '(.*)'")]
		public void GivenThereIsAShiftBagNamedWithRuleSet(string name, string ruleset)
		{
			DataMaker.Data().Apply(new RuleSetBagConfigurable
				{
					Name = name,
					RuleSet = ruleset
				});
		}

	}
}