using System;
using System.Drawing;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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

		[When(@"the browser time is '(.*)'")]
		public void WhenTheBrowserTimeIs(DateTime time)
		{
			const string setJsDateTemplate =
				@"Date.prototype.getTime = function () {{ return new Date({0}, {1}, {2}, {3}, {4}, {5}); }};";
			var setJsDate = string.Format(setJsDateTemplate, time.Year, time.Month - 1, time.Day, time.Hour, time.Minute, time.Second);
			Browser.Interactions.Javascript(setJsDate);
		}

		private static void assertRealTimeAgentDetails(string name, RealTimeAdherenceAgentStateInfo stateInfo)
		{
			const string selector = ".agent-name:contains('{0}') ~ td:contains('{1}')";

			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.State);
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.Activity);
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.NextActivity);
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.NextActivityStartTimeFormatted());
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.Alarm);
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.AlarmTimeFormatted());
			Browser.Interactions.AssertExistsUsingJQuery(selector, name, stateInfo.TimeInStateFormatted());

			const string colorSelector = "tr[style*='background-color: {0}'] .agent-name:contains('{1}')";
			Browser.Interactions.AssertExistsUsingJQuery(colorSelector, toRGBA(stateInfo.AlarmColor, "0.6"), name);

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
			var format = DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
			return DateTime.Parse(NextActivityStartTime).ToString(format);
		}

		public string AlarmTimeFormatted()
		{
			var format = DataMaker.Me().Culture.DateTimeFormat.ShortTimePattern;
			return DateTime.Parse(AlarmTime).ToString(format);
		}

		public string TimeInStateFormatted()
		{
			return TimeSpan.Parse(TimeInState).ToString(@"h\:mm\:ss");
		}
	}
}