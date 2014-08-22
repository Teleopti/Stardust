using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class TeamStepDefinitions
	{
		[Given(@"there is a team with")]
		public void GivenThereIsATeamWith(Table table)
		{
			DataMaker.ApplyFromTable<TeamConfigurable>(table);
		}

		[Given(@"there is a team named '(.*)' on site '(.*)'")]
		[Given(@"there is a team named '(.*)' on '(.*)'")]
		public void GivenThereIsATeamNamedOnSite(string name, string site)
		{
			DataMaker.Data().Apply(new TeamConfigurable
				{
					Name = name,
					Site = site
				});
		}

	}
}