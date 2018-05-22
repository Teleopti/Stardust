using System;
using System.Globalization;
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
		[When(@"I view historical adherence for '(\D*)'")]
		public void WhenIViewHistoricalAdherenceFor(string name)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GoToAgentHistoricalAdherence(personId);
		}

		[When(@"I view historical adherence for '(\D*)' on '(.*)'")]
		public void WhenIViewHistoricalAdherenceForOn(string name, string date)
		{
			TestControllerMethods.Logon();
			var personId = DataMaker.Person(name).Person.Id.Value;
			Navigation.GoToAgentHistoricalAdherence(personId, DateTime.Parse(date));
		}

		[When(@"I approve out of adherence starting at '(.*)' as in adherence")]
		public void WhenIApproveAdherenceAsInAdherenceBetween(string from)
		{
			Browser.Interactions.ClickContaining(".recorded-out-of-adherence", from);
			Browser.Interactions.Click(".approve-adherence-submit");
		}

		[When(@"I remove approved period between '(.*)' and '(.*)'")]
		public void WhenIRemoveApprovedPeriodBetweenAnd(string from, string to)
		{
			Browser.Interactions.Click($".remove-approved-period[data-starttime='{from}'][data-endtime='{to}']");
		}

		[Then(@"I should see out of adherences")]
		public void ThenIShouldSeeOutOfAdherences(Table table)
		{
			var outOfAdherences = table.CreateSet<StartEndTimePair>();
			outOfAdherences.ForEach(x => { Browser.Interactions.AssertExists(".out-of-adherence[data-starttime='{0}'][data-endtime='{1}']", x.StartTime, x.EndTime); });
		}

		[Then(@"I should see out of adherence between '(.*)' and '(.*)'")]
		public void ThenIShouldSeeOutOfAdherenceBetweenAnd(string from, string to)
		{
			Browser.Interactions.AssertExists(".out-of-adherence[data-starttime='{0}'][data-endtime='{1}']", from, to);
		}

		[Then(@"I should see recorded out of adherence between '(.*)' and '(.*)'")]
		public void ThenIShouldSeeRecordedOutOfAdherenceBetween(string from, string to)
		{
			Browser.Interactions.AssertAnyContains(".recorded-out-of-adherence", from);
			Browser.Interactions.AssertAnyContains(".recorded-out-of-adherence", to);
		}

		[Then(@"I should see approved period between '(.*)' and '(.*)'")]
		public void ThenIShouldSeeApprovedPeriodBetween(string from, string to)
		{
			Browser.Interactions.AssertAnyContains(".approved-period", from);
			Browser.Interactions.AssertAnyContains(".approved-period", to);
		}

		[Then(@"I should not be able to approve out of adherences")]
		public void ThenIShouldNotBeAbleToApproveOutOfAdherences()
		{
			Browser.Interactions.Click(".recorded-out-of-adherence");
			Browser.Interactions.AssertNotExists(".recorded-out-of-adherence", ".approve-adherence-submit");
		}

		[Then(@"I should be able to approve out of adherences")]
		public void ThenIShouldBeAbleToApproveOutOfAdherences()
		{
			Browser.Interactions.Click(".recorded-out-of-adherence");
			Browser.Interactions.AssertExists(".approve-adherence-submit");
		}

		[Then(@"I should not be able to remove approved out of adherences")]
		public void ThenIShouldNotBeAbleToRemoveOutOfAdherences()
		{
			Browser.Interactions.Click(".approved-period");
			Browser.Interactions.AssertNotExists(".approved-period", ".remove-approved-period");
		}

		[Then(@"I should not see any out of adherences")]
		public void ThenIShouldNotSeeAnyOutOfAdherences()
		{
			Browser.Interactions.AssertNotExists(".rta-view", ".out-of-adherence");
		}

		[Then(@"I should see activities")]
		public void ThenIShouldSeeActivities(Table table)
		{
			var activities = table.CreateSet<StartEndTimePair>();
			activities.ForEach(x => { Browser.Interactions.AssertExists(".activity[data-starttime='{0}'][data-endtime='{1}']", x.StartTime, x.EndTime); });
		}

		[Then(@"I should see adherence percentage of (.*)%")]
		public void ThenIShouldSeeHistoricalAdherenceForWithAdherenceOf(string adherence)
		{
			Browser.Interactions.AssertFirstContains(".adherence-percent", adherence);
		}
		
		[Then(@"I should be informed she is late for work with (.*) minutes")]
		public void ThenIShouldBeInformedSheIsLateForWorkWithMinutes(string minutes) =>
			Browser.Interactions.AssertFirstContains(".late-for-work", minutes);

		[Then(@"I should not be informed she is late for work")]
		public void ThenIShouldNotBeInformedSheIsLateForWork() =>
			Browser.Interactions.AssertNotExists(".change", ".late-for-work");

		[Then(@"I should rule and state changes")]
		public void ThenIShouldRuleAndStateChanges(Table table) =>
			table.CreateSet<RuleAndStateChanges>().ForEach(assertRuleAndStateChange);

		[Then(@"I should see rule change '(.*)' at '(.*)'")]
		public void ThenIShouldSeeRuleChange(string rule, string time) =>
			assertRuleAndStateChange(new RuleAndStateChanges {Time = time, Rule = rule});

		private static void assertRuleAndStateChange(RuleAndStateChanges change)
		{
			var selector = $".change[data-time='{change.Time}']";

			// because....
			// in some rare cases if you use a table, where the td only has content in a child span
			// sometimes other td:s do not get rendered/or something due to something height something
			// forcing hight "fixes" it
			var forceRowHeight = $@"
var element = document.querySelector(""{selector}"");
element.style.height = '50px';
return 'OK';
";
			Browser.Interactions.AssertJavascriptResultContains(forceRowHeight, "OK");

			Browser.Interactions.AssertFirstContains(selector, change.Time);
			if (!string.IsNullOrEmpty(change.Activity))
				Browser.Interactions.AssertFirstContains(selector, change.Activity);
			if (!string.IsNullOrEmpty(change.State))
				Browser.Interactions.AssertFirstContains(selector, change.State);
			if (!string.IsNullOrEmpty(change.Rule))
				Browser.Interactions.AssertFirstContains(selector, change.Rule);
			if (!string.IsNullOrEmpty(change.AdherenceText))
				Browser.Interactions.AssertFirstContains(selector, change.AdherenceText);
		}

		public class RuleAndStateChanges
		{
			public string Time;
			public string Activity;
			public string State;
			public string Rule;
			public Adherence? Adherence;

			public string AdherenceText
			{
				get
				{
					if (Adherence.HasValue)
						switch (Adherence.Value)
						{
							case Domain.InterfaceLegacy.Domain.Adherence.In:
								return UserTexts.Resources.InAdherence;
							case Domain.InterfaceLegacy.Domain.Adherence.Neutral:
								return UserTexts.Resources.NeutralAdherence;
							case Domain.InterfaceLegacy.Domain.Adherence.Out:
								return UserTexts.Resources.OutOfAdherence;
						}
					return null;
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