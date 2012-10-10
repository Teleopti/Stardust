using System.Collections.Generic;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class WorkShiftRuleSetStepDefinitions
	{
		[StepArgumentTransformation]
		public WorkShiftRuleSetConfigurable WorkShiftRuleSetConfigurableTransform(Table table)
		{
			return table.CreateInstance<WorkShiftRuleSetConfigurable>();
		}

		[Given(@"there is a rule set with")]
		public void GivenThereIsARuleSetWith(WorkShiftRuleSetConfigurable workShiftRuleSet)
		{
			UserFactory.User().Setup(workShiftRuleSet);
		}

	}
}