using NUnit.Framework;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using WatiN.Core;
using Browser = Teleopti.Ccc.WebBehaviorTest.Core.Browser;
using Table = TechTalk.SpecFlow.Table;
using TableRow = TechTalk.SpecFlow.TableRow;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Anywhere
{
	[Binding]
	public class TeamSchedulePageStepDefinitions
	{
		[Then(@"I should be viewing schedules for '(.*)'")]
		public void ThenIShouldSeeAgentScheduleForAgentOnDate(string date)
		{
			EventualAssert.That(() => Browser.Current.Url.Contains(date.Replace("-", "")), Is.True);
		}

		[Then(@"I should see schedule for '(.*)'")]
		public void ThenIShouldSeeScheduleFor(string personName)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".agent:contains('{0}') .shift li", personName))).Exists, Is.True);
		}

		[Then(@"I should see no schedule for '(.*)'")]
		public void ThenIShouldSeeNoScheduleFor(string personName)
		{
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".agent:contains('{0}')", personName))).Exists, Is.True);
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".agent:contains('{0}') .shift li", personName))).Exists, Is.False);
		}

		[Then(@"I should be able to select teams")]
		public void ThenIShouldBeAbleToSelectTeams(Table table)
		{
			foreach (var row in table.Rows)
			{
				EventualAssert.That(() => Browser.Current.Element(Find.BySelector(string.Format(".team-selector:contains('{0}'", row["Team"]))).Exists, Is.True);
			}
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			Browser.Current.Element(Find.BySelector(string.Format(".team-selector li:contains('{0}", teamName))).EventualClick();
		}
	}
}