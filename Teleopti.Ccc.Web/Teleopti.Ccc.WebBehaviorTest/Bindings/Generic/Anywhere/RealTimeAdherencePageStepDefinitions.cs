using System;
using System.Drawing;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

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
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".site [data-value='{0}']:contains('{1}')", numberOfOutAdherence, site));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".site [data-max='{0}']:contains('{1}')", total, site));
		}

		[Then(@"I should see team '(.*)' with (.*) of (.*) employees out of adherence")]
		public void ThenIShouldSeeTeamWithOfEmployeesOutOfAdherence(string team, int numberOfOutAdherence, int total)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".team [data-value='{0}']:contains('{1}')", numberOfOutAdherence, team));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".team [data-max='{0}']:contains('{1}')", total, team));
		}

		[Then(@"I should see real time agent details for '(.*)'")]
		public void ThenIShouldSeeRealTimeAgentDetailsFor(string name, Table table)
		{
			var stateInfo = table.CreateInstance<RealTimeAdherenceAgentStateInfo>();
			assertRealTimeAgentDetails(name,stateInfo);
		}

		[Then(@"I should see real time agent name for '(.*)'")]
		public void ThenIShouldSeeRealTimeAgentNameFor(string name)
		{
			assertRealTimeAgentName(name);
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
			var persons = table.CreateSet<RealTimeAdherenceAgentStateInfo>();
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
			Browser.Interactions.Click(string.Format(".agent-menu a.{0}", cssClass.Name));
		}

		[Then(@"I should see historical adherence for '(.*)' with adherence of (.*)%")]
		public void ThenIShouldSeeHistoricalAdherenceForWithAdherenceOf(string person, int adherence)
		{
			const string selector = ".historical-adherence:contains('{0}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, adherence);
		}

		[Then(@"I should see daily adherence for '(.*)' is (.*)%")]
		public void ThenIShouldSeeDailyAdherenceForIs(string person, int adherence)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".agent-name:contains('{0}')", person);
			Browser.Interactions.AssertExistsUsingJQuery(".daily-percent:contains('{0}')", adherence);
		}

		[Then(@"I should see '(.*)' with adherence of (.*)%")]
		public void ThenIShouldSeeWithAdherenceOf(string activity, int adherence)
		{
			Browser.Interactions.AssertExistsUsingJQuery(".activity-name:contains('{0}')", activity);
			Browser.Interactions.AssertExistsUsingJQuery(".adherence-percent:contains('{0}')", adherence);
		}

		[When(@"I click the site checkbox for '(.*)'")]
		[When(@"I click the team checkbox for '(.*)'")]
		public void WhenIClickTheCheckboxFor(string name)
		{
			Browser.Interactions.ClickUsingJQuery(string.Format(".checkbox[name='{0}']", name));
		}

		[When(@"I choose business unit '(.*)'")]
		public void WhenIChooseBusinessUnit(string businessUnitName)
		{
			Browser.Interactions.ClickUsingJQuery(string.Format("span:contains('{0}')", businessUnitName));
		}

		private static void assertRealTimeAgentDetails(string name, RealTimeAdherenceAgentStateInfo stateInfo)
		{
			const string selector = ".agent-name:contains('{0}') ~ :contains('{1}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.State);
			if (stateInfo.Activity != null)	
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.Activity);
			if (stateInfo.NextActivity != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.NextActivity);
			if (stateInfo.NextActivityStartTimeFormatted() != null )
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.NextActivityStartTimeFormatted());
			if (stateInfo.Alarm != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.Alarm);
			if (stateInfo.AlarmTimeFormatted() != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.AlarmTimeFormatted());

			if (stateInfo.AlarmColor != null)
			{
				const string colorSelector = "tr[style*='background-color: {0}'] .agent-name:contains('{1}')";
				Browser.Interactions.AssertExistsUsingJQuery(colorSelector, toRGBA(stateInfo.AlarmColor, "0.6"), name);
			}
		}

		private static void assertRealTimeAgentName(string name)
		{
			const string selector = ".agent-name:contains('{0}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, name);
		}

		private static String toRGBA(string colorName, string transparency)
		{
			var color = Color.FromName(colorName);
			return string.Format("rgba({0}, {1}, {2}, {3})", color.R, color.G, color.B, transparency);
		}
	}

	public class RealTimeAdherenceAgentStateInfo
	{
		public string Name	{ get; set; }
		public string State	{ get; set; }
		public string Activity	{ get; set; }
		public string NextActivity	{ get; set; }
		public string NextActivityStartTime	{ get; set; }
		public string Alarm	{ get; set; }
		public string AlarmColor { get; set; }
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