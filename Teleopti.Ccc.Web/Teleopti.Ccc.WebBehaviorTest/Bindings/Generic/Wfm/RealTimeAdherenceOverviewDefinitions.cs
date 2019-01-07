using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceOverviewDefinitions
	{
		[Then(@"I should see site '(.*)'")]
		public void ThenIShouldSeeSite(string siteName)
		{
			Browser.Interactions.AssertAnyContains(".site", siteName);
		}

		[Then(@"I should see site '(.*)' with (.*) of (.*) employees out of adherence")]
		[Then(@"I should see site '(.*)' with (.*) of (.*) agents in alarm")]
		public void ThenIShouldSeeSiteWithOfEmployeesOutOfAdherence(string site, int number, int total)
		{
			Browser.Interactions.AssertAnyContains($".site [data-value='{number}']", site);
			Browser.Interactions.AssertAnyContains($".site [data-max='{total}']", site);
		}

		[Then(@"I should see site '(.*)' with (.*) employees out of adherence")]
		[Then(@"I should see site '(.*)' with (.*) agents in alarm")]
		public void ThenIShouldSeeSiteWithOfEmployeesOutOfAdherence2(string site, int number)
		{
			Browser.Interactions.AssertAnyContains($".site [data-value='{number}']", site);
		}

		[Then(@"I should not see site '(.*)'")]
		public void ThenIShouldNotSeeSite(string siteName)
		{
			Browser.Interactions.AssertNotExists("body", $"[name='{siteName}']");
		}

		[Then(@"I should see team '(.*)'")]
		public void ThenIShouldSeeTeam(string teamName)
		{
			Browser.Interactions.AssertAnyContains(".team", teamName);
		}

		[Then(@"I should see team '(.*)' with (.*) of (.*) employees out of adherence")]
		[Then(@"I should see team '(.*)' with (.*) of (.*) agents in alarm")]
		public void ThenIShouldSeeTeamWithOfEmployeesOutOfAdherence(string team, int number, int total)
		{
			Browser.Interactions.AssertAnyContains($".team [data-value='{number}']", team);
			Browser.Interactions.AssertAnyContains($".team [data-max='{total}']", team);
		}

		[Then(@"I should see team '(.*)' with (.*) employees out of adherence")]
		[Then(@"I should see team '(.*)' with (.*) agents in alarm")]
		public void ThenIShouldSeeTeamWithOfEmployeesOutOfAdherence2(string team, int number)
		{
			Browser.Interactions.AssertAnyContains($".team [data-value='{number}']", team);
		}

		[Then(@"I should not see team '(.*)'")]
		public void ThenIShouldNotSeeTeam(string teamName)
		{
			Browser.Interactions.AssertNotExists("body", $"[name='{teamName}']");
		}

		[Then(@"I should not be able to modify skill groups")]
		public void ThenIShouldNotBeAbleToModifySkillGroups()
		{
			Browser.Interactions.AssertNotExists(".rta-filter-panel, .organization-picker", "#manage_skill_group_button");
		}

		[Then(@"I should be able to modify skill groups")]
		public void ThenIShouldBeAbleToModifySkillGroups()
		{
			Browser.Interactions.AssertExists("#manage_skill_group_button");
		}
	}
}