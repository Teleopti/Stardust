using System;
using System.Drawing;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Adherence
{
	[Binding]
	public class AgentsDefinitions
	{
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
			if (status.Rule != null)
				Browser.Interactions.AssertExistsUsingJQuery(selector, name, status.Rule);
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

		[Then(@"I should see an agent '(.*)'")]
		public void ThenIShouldSeeAgent(string agent)
		{
			var personId = DataMaker.Person(agent).Person.Id.Value;
			Browser.Interactions.AssertExists("body", "[agentid='" + personId + "']");
		}


		[Then(@"I should not see agent '(.*)'")]
		public void ThenIShouldNotSeeAgent(string agent)
		{
			var personId = DataMaker.Person(agent).Person.Id.Value;
			Browser.Interactions.AssertNotExists("body", "[agentid='" + personId + "']");
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
			var status = new RealTimeAdherenceAgentState() {Name = name};
			assertAgentStatus(status);
		}

		[Then(@"I should see agent '(.*)' with state '(.*)'")]
		public void ThenIShouldSeeAgentStatusForWithState(string name, string stateCode)
		{
			var status = new RealTimeAdherenceAgentState() {Name = name, State = stateCode};
			assertAgentStatus(status);
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

		private static void assertAgentStatus(RealTimeAdherenceAgentState state)
		{
			var personId = DataMaker.Person(state.Name).Person.Id.Value;
			var selector = "[agentid='" + personId + "']";

			Browser.Interactions.AssertExists(selector);
			if (state.Name != null)
				Browser.Interactions.AssertAnyContains(selector, state.Name);
			if (state.State != null)
				Browser.Interactions.AssertAnyContains(selector, state.State);
			if (state.Rule != null)
				Browser.Interactions.AssertAnyContains(selector, state.Rule);

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
			if (state.OutOfAdherenceTimeFormatted() != null)
				Browser.Interactions.AssertAnyContains(selector, state.OutOfAdherenceTimeFormatted());


			if (state.AlarmColor != null)
				Browser.Interactions.AssertExists(selector + " [style*='background-color: " + toHex(state.AlarmColor) + "']");
			if (state.Color != null)
				Browser.Interactions.AssertExists(selector + " [style*='background-color: " + toHex(state.Color) + "']");
		}

		private static string toRGBA(string colorName, string transparency)
		{
			var color = Color.FromName(colorName);
			return $"rgba({color.R}, {color.G}, {color.B}, {transparency})";
		}

		private static string toHex(string colorName)
		{
			var c = Color.FromName(colorName);
			return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
		}
	}

	public class RealTimeAdherenceAgentState
	{
		public string Name { get; set; }
		public string State { get; set; }
		public string PreviousActivity { get; set; }
		public string Activity { get; set; }
		public string NextActivity { get; set; }
		public string NextActivityStartTime { get; set; }
		public string Rule { get; set; }
		public string AlarmColor { get; set; }
		public string Color { get; set; }
		public string AlarmTime { get; set; }
		public string OutOfAdherenceTime { get; set; }

		public string NextActivityStartTimeFormatted()
		{
			return NextActivityStartTime == null ? null : DateTime.Parse(NextActivityStartTime).ToString(@"HH\:mm");
		}

		public string AlarmTimeFormatted()
		{
			return formatTime(AlarmTime);
		}

		public string OutOfAdherenceTimeFormatted()
		{
			return formatTime(OutOfAdherenceTime);
		}

		private static string formatTime(string time)
		{
			return time == null ? null : TimeSpan.Parse(time).ToString(@"h\:mm\:ss");
		}
	}
}