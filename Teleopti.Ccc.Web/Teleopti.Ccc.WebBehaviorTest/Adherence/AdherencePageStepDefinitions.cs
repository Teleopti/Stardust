using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Bindings;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class AdherencePageStepDefinitions
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
			Browser.Interactions.CloseOtherTabs_Experimental();
		}

		[Then(@"I should be able to change the scehdule for '(.*)'")]
		public void ThenIShouldSeeTheOptionToChangeSchedule(string name)
		{
			Browser.Interactions.AssertExists($".change-schedule");
			Browser.Interactions.Click($".change-schedule");
			Browser.Interactions.SwitchToLastTab_Experimental();
			Browser.Interactions.AssertFirstContains("td.person-name-column", name);
//			Browser.Interactions.AssertExists("td.schedule .layer");
//			Browser.Interactions.AssertExists("td.schedule-tool-column span i");
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