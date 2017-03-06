using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceOverviewDefinitions
	{
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


		[When(@"I click the site checkbox for '(.*)'")]
		[When(@"I click the team checkbox for '(.*)'")]
		public void WhenIClickTheCheckboxFor(string name)
		{
			Browser.Interactions.Click($"[name='{name}']");
		}
	}
}