using System;
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

		[Then(@"I should see real time agent details")]
		public void ThenIShouldSeeRealTimeAgentDetails(Table table)
		{
			var stateInfo = table.CreateInstance<RealTimeAdherenceAgentStateInfo>();
			assertRealTimeAgentDetails(stateInfo);
		}

		private static void assertRealTimeAgentDetails(RealTimeAdherenceAgentStateInfo stateInfo)
		{
			Browser.Interactions.AssertExistsUsingJQuery(string.Format("TD.agent-name:contains('{0}')", stateInfo.Name));
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
		public string AlarmTime	{ get; set; }
		public TimeSpan TimeInState	 { get; set; }
	}
}