using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class SkillStepDefinitions
	{
		[Given(@"there is a skill with")]
		public void GivenThereIsASkillWith(Table table)
		{
			var skill = table.CreateInstance<SkillConfigurable>();
			DataMaker.Data().Apply(skill);
		}

	}
}