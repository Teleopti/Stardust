using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class RuleStepDefinitions
	{
		[Given(@"there is a rule with")]
		[Given(@"there is an alarm with")]
		public void GivenThereIsARuleWith(Table table)
		{
			DataMaker.Data().Apply(table.CreateInstance<RtaMapConfigurable>());
		}

		[Given(@"there is a rule named '(.*)' with")]
		public void GivenThereIsARuleNamedWith(string name, Table table)
		{
			var rule = table.CreateInstance<RtaMapConfigurable>();
			rule.Name = name;
			DataMaker.Data().Apply(rule);
		}

	}
}