using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

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


		[Given(@"there is a skill in timezone '(.*)' named '(.*)' with activity '(.*)'")]
		public void GivenThereIsASkillNamedWithActivity( string timezone, string name, string activity)
		{
			DataMaker.Data().Apply(new SkillConfigurable
			{
				Name = name,
				Activity = activity,
				TimeZone = timezone
			});
		}
		

	}
}