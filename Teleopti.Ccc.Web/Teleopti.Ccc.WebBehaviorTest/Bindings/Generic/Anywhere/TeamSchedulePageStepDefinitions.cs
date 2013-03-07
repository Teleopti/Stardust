using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Collection;
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
			var select = Browser.Current.SelectList(Find.BySelector(".team-selector"));
			EventualAssert.That(() => select.Exists, Is.True);

			var teams = table.CreateSet<TeamInfo>();
			teams.ForEach(t => EventualAssert.That(() => select.Option(Find.BySelector(string.Format(":contains('{0}')", t.Team))).Exists, Is.True));
		}
		
		public class TeamInfo
		{
			public string Team { get; set; }
		}

		[When(@"I select team '(.*)'")]
		public void WhenISelectTeam(string teamName)
		{
			var select = Browser.Current.SelectList(Find.BySelector(".team-selector"));
			EventualAssert.That(() => select.Exists, Is.True);
			
			select.Option(Find.BySelector(string.Format(":contains('{0}')", teamName))).SelectNoWait();
			//Browser.Current.SelectList(Find.BySelector(".team-selector")).Option(Find.BySelector(string.Format(":contains('{0}')", teamName))).SelectNoWait();
		}

		[When(@"I select date '(.*)'")]
		public void WhenISelectDate(string date)
		{
			Browser.Current.Element(Find.BySelector(".icon-calendar")).EventualClick();
			
			EventualAssert.That(() => Browser.Current.Element(Find.BySelector(".datepicker")).Style.Display == "none", Is.False);

			var dateParts = date.Split('-');

			//Select datepicker year
			Browser.Current.Element(Find.BySelector(".datepicker .switch")).EventualClick();
			Browser.Current.Element(Find.BySelector(".datepicker .switch")).EventualClick();
			Browser.Current.Element(Find.BySelector(string.Format(".datepicker-years .year:contains('{0}')", dateParts[0]))).EventualClick();

			//Select datepicker month
			Browser.Current.Element(Find.BySelector(string.Format(".datepicker-months .month:nth-child({0})", dateParts[1]))).EventualClick();

			//Select datepicker day
			Browser.Current.Elements.Filter(Find.BySelector(".datepicker-days .day")).Filter(Find.ByText(t => t.Equals(dateParts[2].TrimStart('0')))).First().EventualClick();
		}
	}
}