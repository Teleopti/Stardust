using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Generic;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TeamStepDefinitions
	{
		[Given(@"there is a team with")]
		public void GivenThereIsATeamWith(Table table)
		{
			var team = table.CreateInstance<TeamConfigurable>();
			UserFactory.User().Setup(team);
		}

	}
}