using System;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class Transform
	{
		[StepArgumentTransformation]
		public static TimePeriod ToTimePeriod(string value)
		{
			var values = value.Split('-');
			var start = ToTimeSpan(values[0]);
			var end = start;
			if (values.Length > 1)
				end = ToTimeSpan(values[1]);
			return new TimePeriod(start, end);
		}

		[StepArgumentTransformation]
		public static TimeSpan ToTimeSpan(string value)
		{
			return TimeSpan.Parse(value);
		}

		[StepArgumentTransformation]
		public static TimeSpan? ToNullableTimeSpan(string value)
		{
			if (value == null)
				return null;
			return TimeSpan.Parse(value);
		}
	}

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
			UserFactory.User().Setup(workShiftRuleSet);
		}

		[Given(@"there is a rule set bag with")]
		public void GivenThereIsARuleSetBagWith(RuleSetBagConfigurable ruleSetBag)
		{
			UserFactory.User().Setup(ruleSetBag);
		}

	}
}