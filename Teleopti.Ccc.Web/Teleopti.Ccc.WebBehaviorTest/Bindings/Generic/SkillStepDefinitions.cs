using TechTalk.SpecFlow;
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
			DataMaker.ApplyFromTable<SkillConfigurable>(table);
		}

		[Given(@"there is a skill named '(.*)' with activity '(.*)'")]
		public void GivenThereIsASkillNamedWithActivity(string name, string activity)
		{
			DataMaker.Data().Apply(new SkillConfigurable
				{
					Name = name,
					Activity = activity
				});
		}

	}
}