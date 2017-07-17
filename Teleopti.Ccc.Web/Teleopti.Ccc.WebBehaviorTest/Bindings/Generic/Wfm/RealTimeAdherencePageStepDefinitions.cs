using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceToolPageStepDefinitions
	{
		[When(@"I send '(.*)' for all agents")]
		public void WhenIClickFor(string state)
		{
			Browser.Interactions.ClickContaining(".sendbatch", state.ToUpper());
			Browser.Interactions.AssertExists(".notice-info");
		}
	}

	[Binding]
	public class RealTimeAdherencePageStepDefinitions
	{
		[When(@"I click( the)? ([a-z-]*|[a-z]* [a-z]*) of '(.*)'")]
		public void WhenIClickClassWithText(string the, CssClass cssClass, string text)
		{
			Browser.Interactions.ClickContaining("." + cssClass.Name, text);
		}
		
		[Then(@"I should see a message that I have no permission for this function")]
		public void ThenIShouldSeeAMessageThatIHaveNoPermissionForThisFunction()
		{
			Browser.Interactions.AssertExists(".error-message");
		}

		[Then(@"I should see Real time adherence overview in the menu")]
		public void ThenIShouldSeeRealTimeAdherenceOverviewInTheMenu()
		{
			Browser.Interactions.AssertExists("#link-realtimeadherence");
		}

		[Then(@"I should not see Real time adherence overview in the menu")]
		public void ThenIShouldNotSeeRealTimeAdherenceOverviewInTheMenu()
		{
			Browser.Interactions.AssertNotExists(".menu", "#link-realtimeadherence");
		}

		[When(@"I click on an agent state")]
		public void WhenIClickOnAnAgentState()
		{
			Browser.Interactions.Click(".agent-state");
		}

		[When(@"I choose to continue")]
		public void WhenIChooseToContinue()
		{
			Browser.Interactions.Click(".send-message");
		}

		[Then(@"The message tool should be opened in a new window")]
		public void ThenTheMessageToolShouldBeOpenedInANewWindow()
		{
			Browser.Interactions.AssertUrlNotContains("Anywhere", "ids");
			Browser.Interactions.CloseWindow("Messages");
		}


		[When(@"I click '([a-z-]*|[a-z]* [a-z]*)' in agent menu")]
		public void WhenIClickInAgentMenu(CssClass cssClass)
		{
			Browser.Interactions.Click($".{cssClass.Name}");
		}

		[Then(@"I can '(.*)' in agent menu")]
		public void ThenICanInAgentMenu(CssClass cssClass)
		{
			Browser.Interactions.AssertExists($".{cssClass.Name}");
		}
		
		[Then(@"I should see adherence percentage for '(.*)' at (.*)%")]
		public void ThenIShouldSeeHistoricalAdherenceForWithAdherenceOf(string person, string adherence)
		{
			const string selector = ".historical-adherence";

			Browser.Interactions.AssertAnyContains(selector, adherence);
		}

		[Then(@"I should see daily adherence for '(.*)' is (.*)%")]
		public void ThenIShouldSeeDailyAdherenceForIs(string person, string adherence)
		{
			Browser.Interactions.AssertAnyContains(".agent-name", person);
			Browser.Interactions.AssertAnyContains(".daily-percent", adherence);
		}

		[Then(@"I should see '(.*)' with adherence of (.*)%")]
		public void ThenIShouldSeeWithAdherenceOf(string activity, string adherence)
		{
			Browser.Interactions.AssertAnyContains(".activity-name", activity);
			Browser.Interactions.AssertAnyContains(".adherence-percent", adherence);
		}

		[Then(@"I should see '(.*)' without adherence")]
		public void ThenIShouldSeeWithoutAdherence(string activity)
		{
			const string noAdherence = "-";
			Browser.Interactions.AssertAnyContains(".activity-name", activity);
			Browser.Interactions.AssertAnyContains(".adherence-percent", noAdherence);
		}

		[When(@"I click the toggle to see all agents")]
		public void WhenIClickTheToggleToSeeAllAgents()
		{
			Browser.Interactions.Click(".rta-agents-in-alarm-switch");
		}
		
		[When(@"I choose business unit '(.*)'")]
		public void WhenIChooseBusinessUnit(string businessUnitName)
		{
			Browser.Interactions.ClickUsingJQuery($"span:contains('{businessUnitName}')");
		}

		[When(@"I change to business unit '(.*)'")]
		public void WhenIChangeToBusinessUnit(string businessUnitName)
		{
			Browser.Interactions.Click("#business-unit-select");
			Browser.Interactions.ClickUsingJQuery($"#business-unit-select li:contains({businessUnitName})");
		}

		[When(@"I select skill area '(.*)'")]
		public void WhenISelectSkillArea(string skillAreaName)
		{
			Browser.Interactions.AssertExists("[skillAreasLoaded='true']");
			Browser.Interactions.Click("#skill-area-input");
			Browser.Interactions.ClickContaining(".rta-skill-area-item", skillAreaName);
		}

		[When(@"I select skill '(.*)'")]
		public void WhenISelectSkill(string skillName)
		{
			Browser.Interactions.AssertExists("[skillsLoaded='true']");
			Browser.Interactions.Click("#skill-input");
			Browser.Interactions.ClickContaining(".rta-skill-item", skillName);
		}
	}
}