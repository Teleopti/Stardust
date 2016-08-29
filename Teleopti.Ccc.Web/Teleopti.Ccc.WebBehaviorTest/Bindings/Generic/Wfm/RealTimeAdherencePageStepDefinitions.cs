using System;
using System.Drawing;
using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class RealTimeAdherencePageStepDefinitions
	{
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

		[Then(@"I should see real time agent details for '(.*)'")]
		public void ThenIShouldSeeRealTimeAgentDetailsFor(string name, Table table)
		{
			var status = table.CreateInstance<RealTimeAdherenceAgentState>();

			const string selector = ".agent-name:contains('{0}') ~ :contains('{1}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.State);
			if (status.Activity != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.Activity);
			if (status.NextActivity != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.NextActivity);
			if (status.NextActivityStartTimeFormatted() != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.NextActivityStartTimeFormatted());
			if (status.Alarm != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.Alarm);
			if (status.AlarmTimeFormatted() != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.AlarmTimeFormatted());
			if (status.AlarmColor != null)
				Browser.Interactions.AssertExistsUsingJQuery("tr[style*='background-color: {0}'] .agent-name:contains('{1}')", toRGBA(status.AlarmColor, "0.6"), name);
		}

		[Then(@"I should see agent status")]
		public void ThenIShouldSeeAgentDetailsFor(Table table)
		{
			var status = table.CreateInstance<RealTimeAdherenceAgentState>();
			assertAgentStatus(status);
		}
		
		[Then(@"I should not see agent '(.*)'")]
		public void ThenIShouldNotSeeAgent(string agent)
		{
			var personId = DataMaker.Person(agent).Person.Id.Value;
			Browser.Interactions.AssertNotExists("body","[agentid='" + personId + "']");
		}

		[Then(@"I should see agent '(.*)' before '(.*)'")]
		public void ThenIShouldSeeAgentBefore(string firstPerson, string secondPerson)
		{
			var firstPersonId = DataMaker.Person(firstPerson).Person.Id.Value;
			var secondPersonId = DataMaker.Person(secondPerson).Person.Id.Value;
			Browser.Interactions.AssertXPathExists($@"//*[@agentid='{firstPersonId}']/following::*[@agentid='{secondPersonId}']");
		}

		[Then(@"I should see agent status for '(.*)?'")]
		public void ThenIShouldSeeAgentStatusFor(string name)
		{
			var status = new RealTimeAdherenceAgentState() { Name = name };
			assertAgentStatus(status);
		}

		[Then(@"I should see agent '(.*)' with state '(.*)'")]
		public void ThenIShouldSeeAgentStatusForWithState(string name, string stateCode)
		{
			var status = new RealTimeAdherenceAgentState() { Name = name, State = stateCode };
			assertAgentStatus(status);
		}

		[Then(@"I should see the menu with option of change schedule")]
		public void ThenIShouldSeeTheMenuWithOptionOfChangeSchedule()
		{
			Browser.Interactions.AssertExists(".change-schedule");
		}

		[When(@"I click( the)? ([a-z-]*|[a-z]* [a-z]*) of '(.*)'")]
		public void WhenIClickClassWithText(string the, CssClass cssClass, string text)
		{
			Browser.Interactions.ClickContaining("." + cssClass.Name, text);
		}

		[When(@"I click on an agent state")]
		public void WhenIClickOnAnAgentState()
		{
			Browser.Interactions.Click(".agent-state");
		}

		[When(@"I select")]
		public void WhenISelect(Table table)
		{
			Browser.Interactions.AssertExists(".send-message");
			var persons = table.CreateSet<RealTimeAdherenceAgentState>();
			foreach (var person in persons)
			{
				Browser.Interactions.ClickUsingJQuery(".agent-state:has(.agent-name:contains('" + person.Name + "')) .selectable-agent input");
			}
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
		
		[Then(@"I should see historical adherence for '(.*)' with adherence of (.*)%")]
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

		[When(@"I click the site checkbox for '(.*)'")]
		[When(@"I click the team checkbox for '(.*)'")]
		public void WhenIClickTheCheckboxFor(string name)
		{
			Browser.Interactions.Click($"[name='{name}']");
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
			Browser.Interactions.ClickContaining("md-option", businessUnitName);
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


		private static void assertAgentStatus(RealTimeAdherenceAgentState state)
		{
			var personId = DataMaker.Person(state.Name).Person.Id.Value;
			var selector = "[agentid='" + personId + "']";

			Browser.Interactions.AssertExists(selector);
			if (state.State != null)
				Browser.Interactions.AssertAnyContains(selector, state.State);
			if (state.Alarm != null)
				Browser.Interactions.AssertAnyContains(selector, state.Alarm);

			if (state.PreviousActivity != null)
			{
				if (state.PreviousActivity == "<none>")
					Browser.Interactions.AssertNotExists(selector, selector + " .previous-activity");
				else
					Browser.Interactions.AssertExists(selector + " .previous-activity[name='{0}']", state.PreviousActivity);
			}
			if (state.Activity != null)
				Browser.Interactions.AssertExists(selector + " .current-activity[name='{0}']", state.Activity);
			if (state.NextActivity != null)
				Browser.Interactions.AssertExists(selector + " .next-activity[name='{0}']", state.NextActivity);

			if (state.NextActivityStartTimeFormatted() != null)
			{
				Assert.Fail("Enable this assert and remove from scenarios when toggle is removed");
			}

			if (state.AlarmColor != null)
				Browser.Interactions.AssertExists(selector + " [style*='background-color: " + toRGBA(state.AlarmColor, "0.6") + "']");
			if (state.Color != null)
				Browser.Interactions.AssertExists(selector + " [style*='background-color: " + toRGBA(state.Color, "0.6") + "']");

			if (state.RuleTimeFormatted() != null)
			{

				Assert.Fail("Enable this assert and remove from scenarios when toggle is removed");

			}
			if (state.AlarmTimeFormatted() != null)
			{

				Assert.Fail("Enable this assert and remove from scenarios when toggle is removed");

			}
			if (state.OutOfAdherenceTimeFormatted() != null)
				Browser.Interactions.AssertAnyContains(selector, state.OutOfAdherenceTimeFormatted());
		}
		
		private static string toRGBA(string colorName, string transparency)
		{
			var color = Color.FromName(colorName);
			return $"rgba({color.R}, {color.G}, {color.B}, {transparency})";
		}
	}

	public class RealTimeAdherenceAgentState
	{
		public string Name	{ get; set; }
		public string State	{ get; set; }
		public string PreviousActivity	{ get; set; }
		public string Activity	{ get; set; }
		public string NextActivity	{ get; set; }
		public string NextActivityStartTime	{ get; set; }
		public string Alarm	{ get; set; }
		public string AlarmColor { get; set; }
		public string Color { get; set; }
		public string AlarmTime	{ get; set; }
		public string RuleTime { get; set; }
		public string TimeInState { get; set; }
		public string OutOfAdherenceTime { get; set; }

		public string NextActivityStartTimeFormatted()
		{
			return NextActivityStartTime == null ? null : DateTime.Parse(NextActivityStartTime).ToString(@"HH\:mm");
		}

		public string AlarmTimeFormatted()
		{
			return formatTime(AlarmTime);
		}

		public string RuleTimeFormatted()
		{
			return formatTime(RuleTime);
		}

		public string OutOfAdherenceTimeFormatted()
		{
			return formatTime(OutOfAdherenceTime);
		}

		private static string formatTime(string time)
		{
			return time == null ? null : TimeSpan.Parse(time).ToString(@"h\:mm\:ss");
		}

		public string TimeInStateFormatted()
		{
			return TimeSpan.Parse(TimeInState).ToString(@"h\:mm\:ss");
		}
	}
}