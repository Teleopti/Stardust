using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
				var selector = $".change[data-time='{change.Time}']";
				Browser.Interactions.AssertFirstContains(selector, change.Time);
				if (!string.IsNullOrEmpty(change.Activity)) 
					Browser.Interactions.AssertFirstContains(selector, change.Activity);
				Browser.Interactions.AssertFirstContains(selector, change.State);
				Browser.Interactions.AssertFirstContains(selector, change.Rule);
				Browser.Interactions.AssertFirstContains(selector, change.AdherenceText);
			});
		}

		public class RuleAndStateChanges
		{
			public string Time;
			public string Activity;
			public string State;
			public string Rule;
			public Adherence Adherence;

			public string AdherenceText
			{
				get
				{
					switch (Adherence)
					{
						case Adherence.In:
							return UserTexts.Resources.InAdherence;
						case Adherence.Neutral:
							return UserTexts.Resources.NeutralAdherence;
						case Adherence.Out:
							return UserTexts.Resources.OutOfAdherence;

						default:
							return null;
					}
				}
			}
		}

		public class StartEndTimePair
		{
			public string StartTime;
			public string EndTime;
		}
	}
}