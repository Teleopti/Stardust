using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Navigation;
using Teleopti.Ccc.WebBehaviorTest.Data;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class RealTimeAdherenceAgentHistoricalPageStepDefinitions
	{
		[When(@"I view historical adherence for '(.*)'")]
		public void WhenIViewHistoricalAdherenceFor(string name)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GoToAgentHistoricalAdherence(personId);
		}

		[Then(@"I should see out of adherences")]
		public void ThenIShouldSeeOutOfAdherences(Table table)
		{
			var outOfAdherences = table.CreateSet<StartEndTimePair>();
			outOfAdherences.ForEach(x =>
			{
				Browser.Interactions.AssertExists(".out-of-adherence[data-starttime='{0}'][data-endtime='{1}']", x.StartTime, x.EndTime);
			});
		}

		[Then(@"I should see activities")]
		public void ThenIShouldSeeActivities(Table table)
		{
			var activities = table.CreateSet<StartEndTimePair>();
			activities.ForEach(x =>
			{
				Browser.Interactions.AssertExists(".activity[data-starttime='{0}'][data-endtime='{1}']", x.StartTime, x.EndTime);
			});
		}

		[Then(@"I should rule and state changes")]
		public void ThenIShouldSeeStates(Table table)
		{
			var changes = table.CreateSet<RuleAndStateChanges>();
			changes.ForEach(change =>
			{
				Browser.Interactions.AssertExists(
					".change[data-time='{0}'][data-activity='{1}'][data-state='{2}'][data-rule='{3}'][data-adherence='{4}']",
					change.Time,
					change.Activity,
					change.State,
					change.Rule,
					change.Adherence
				);
			});
		}

		public class RuleAndStateChanges
		{
			public string Time;
			public string Activity;
			public string State;
			public string Rule;
			public string Adherence;
		}

		public class StartEndTimePair
		{
			public string StartTime;
			public string EndTime;
		}
	}
}