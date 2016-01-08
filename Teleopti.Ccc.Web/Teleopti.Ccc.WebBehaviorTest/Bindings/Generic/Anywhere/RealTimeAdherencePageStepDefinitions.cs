using System;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Interfaces.Domain;

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
		public void ThenIShouldSeeSiteWithOfEmployeesOutOfAdherence(string site, int numberOfOutAdherence, int total)
		{
			Browser.Interactions.AssertAnyContains(string.Format(".site [data-value='{0}']", numberOfOutAdherence), site);
			Browser.Interactions.AssertAnyContains(string.Format(".site [data-max='{0}']", total), site);
		}

		[Then(@"I should see team '(.*)' with (.*) of (.*) employees out of adherence")]
		public void ThenIShouldSeeTeamWithOfEmployeesOutOfAdherence(string team, int numberOfOutAdherence, int total)
		{
			Browser.Interactions.AssertAnyContains(string.Format(".team [data-value='{0}']", numberOfOutAdherence), team);
			Browser.Interactions.AssertAnyContains(string.Format(".team [data-max='{0}']", total), team);
		}

		[Then(@"I should see real time agent details for '(.*)'")]
		public void ThenIShouldSeeRealTimeAgentDetailsFor(string name, Table table)
		{
			var stateInfo = table.CreateInstance<RealTimeAdherenceAgentStatus>();
			assertAgentStatus(name,stateInfo);
		}
		
		[Then(@"I should see agent status")]
		public void ThenIShouldSeeAgentDetailsFor(Table table)
		{
			var status = table.CreateInstance<RealTimeAdherenceAgentStatus>();
			assertAgentStatus(status);
		}

		[Then(@"I should see agent '(.*)' before '(.*)'")]
		public void ThenIShouldSeeAgentBefore(string firstPerson, string secondPerson)
		{
			var firstPersonId = DataMaker.Person(firstPerson).Person.Id.Value;
			var secondPersonId = DataMaker.Person(secondPerson).Person.Id.Value;
			Browser.Interactions.AssertXPathExists(string.Format(@"//*[@agentid='{0}']/following::*[@agentid='{1}']",
				firstPersonId, secondPersonId));
		}

		[Then(@"I should see agent status for '(.*)'")]
		public void ThenIShouldSeeAgentStatusFor(string name)
		{
			var status = new RealTimeAdherenceAgentStatus() {Name = name};
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
			var persons = table.CreateSet<RealTimeAdherenceAgentStatus>();
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
			Browser.Interactions.Click(string.Format(".{0}", cssClass.Name));
		}

		[Then(@"I can '(.*)' in agent menu")]
		public void ThenICanInAgentMenu(CssClass cssClass)
		{
			Browser.Interactions.AssertExists(string.Format(".{0}", cssClass.Name));
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
			Browser.Interactions.Click(string.Format("[name='{0}']", name));
		}

		[When(@"I choose business unit '(.*)'")]
		public void WhenIChooseBusinessUnit(string businessUnitName)
		{
			Browser.Interactions.ClickUsingJQuery(string.Format("span:contains('{0}')", businessUnitName));
		}

		[When(@"I change to business unit '(.*)'")]
		public void WhenIChangeToBusinessUnit(string businessUnitName)
		{
			Browser.Interactions.ClickContaining("option", businessUnitName);
		}

		private static void assertAgentStatus(string name, RealTimeAdherenceAgentStatus status)
		{
			const string selector = ".agent-name:contains('{0}') ~ :contains('{1}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.State);
			if (status.Activity != null)	
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.Activity);
			if (status.NextActivity != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.NextActivity);
			if (status.NextActivityStartTimeFormatted() != null )
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.NextActivityStartTimeFormatted());
			if (status.Alarm != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.Alarm);
			if (status.AlarmTimeFormatted() != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.AlarmTimeFormatted());

			if (status.AlarmColor != null)
			{
				const string colorSelector = "tr[style*='background-color: {0}'] .agent-name:contains('{1}')";
				Browser.Interactions.AssertExistsUsingJQuery(colorSelector, toRGBA(status.AlarmColor, "0.6"), name);
			}
		}

		private static void assertAgentStatus(RealTimeAdherenceAgentStatus status)
		{
			var personId = DataMaker.Person(status.Name).Person.Id.Value;
			var selector = "[agentid='" + personId + "']";

			if (status.State != null)
				Browser.Interactions.AssertAnyContains(selector, status.State);
			if (status.Activity != null)
				Browser.Interactions.AssertAnyContains(selector, status.Activity);
			if (status.NextActivity != null)
				Browser.Interactions.AssertAnyContains(selector, status.NextActivity);
			if (status.NextActivityStartTimeFormatted() != null)
				Browser.Interactions.AssertAnyContains(selector, status.NextActivityStartTimeFormatted());
			if (status.Alarm != null)
				Browser.Interactions.AssertAnyContains(selector, status.Alarm);
			if (status.AlarmTimeFormatted() != null)
				Browser.Interactions.AssertAnyContains(selector, status.AlarmTimeFormatted());

			if (status.AlarmColor != null)
			{
				var colorSelector = selector + "[style*='background-color: " + toRGBA(status.AlarmColor, "0.6") +"']";
				Browser.Interactions.AssertExists(colorSelector);
			}
			if (status.Color != null)
			{
				var colorSelector = selector + "[style*='background-color: " + toRGBA(status.Color, "0.6") +"']";
				Browser.Interactions.AssertExists(colorSelector);
			}
		}
		
		private static String toRGBA(string colorName, string transparency)
		{
			var color = Color.FromName(colorName);
			return string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, transparency);
		}
	}

	public class RealTimeAdherenceAgentStatus
	{
		public string Name	{ get; set; }
		public string State	{ get; set; }
		public string Activity	{ get; set; }
		public string NextActivity	{ get; set; }
		public string NextActivityStartTime	{ get; set; }
		public string Alarm	{ get; set; }
		public string AlarmColor { get; set; }
		public string Color { get; set; }
		public string AlarmTime	{ get; set; }
		public string TimeInState	 { get; set; }

		public string NextActivityStartTimeFormatted()
		{
			return NextActivityStartTime == null ? null : DateTime.Parse(NextActivityStartTime).ToString(@"HH\:mm");
		}

		public string AlarmTimeFormatted()
		{
			return AlarmTime == null ? null : TimeSpan.Parse(AlarmTime).ToString(@"h\:mm\:ss");
		}

		public string TimeInStateFormatted()
		{
			return TimeSpan.Parse(TimeInState).ToString(@"h\:mm\:ss");
		}
	}
}