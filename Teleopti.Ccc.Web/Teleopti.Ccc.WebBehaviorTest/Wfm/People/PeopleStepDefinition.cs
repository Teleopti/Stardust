using System;
using System.Linq;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	public static class MyTableExtenstions
	{
		public static string[] AsStrings(this Table t, string column)
		{
			return t.Rows.Select(r => r[column]).ToArray();
		}
	}

	[Binding]
	public sealed class PeopleStepDefinition
	{
		[Given("WA Person '(.*)' exists")]
		public void WAPersonXExists(string name)
		{
			var personPeriod = new PersonPeriodConfigurable()
			{
				StartDate = new DateTime(2014, 2, 21)
			};

			DataMaker.Person(name).Apply(personPeriod);
		}

		[Given("WA Person '(.*)' exists on team '(.*)'")]
		public void WAPersonXExistsOnTeam(string name, string team)
		{
			var personPeriod = new PersonPeriodConfigurable()
			{
				StartDate = new DateTime(2014, 2, 21),
				Team = team
			};

			DataMaker.Person(name).Apply(personPeriod);
		}

		[Given("WA People exists")]
		public void WAPeopleExists(Table table)
		{
			var names = table.AsStrings("Name");
			foreach (var name in names)
				WAPersonXExists(name);
		}

		[Given("I have selected person '(.*)'")]
		[When("I select person '(.*)'")]
		public void ISelectPersonX(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person] [data-test-person-firstname]'))
				.findIndex(p => p.textContent.includes('" + name + "')) !== -1", "True");
			Browser.Interactions.Javascript_IsFlaky(@"
				Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person]')
				).filter(
					p => p.querySelector('[data-test-person-firstname]').textContent.includes('" + name + @"')
				)
				.map(node => node.querySelector('input'))
				.forEach(p => p.click())
				");
		}

		[Given("I have selected people")]
		[When("I select people")]
		public void ISelectPersons(Table table)
		{
			var names = table.AsStrings("Name");
			foreach (var name in names)
				ISelectPersonX(name);
		}

		[Then("I should see '(.*)' in the workspace")]
		public void IShouldSeeXInTheWorkspace(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"return Array.from(document.querySelectorAll('[data-test-workspace] [data-test-person]'))" +
				".findIndex(p => p.textContent.includes('" + name + "')) !== -1", "True");
		}

		[Then("I should not see '(.*)' in the workspace")]
		public void IShouldNotSeeXInTheWorkspace(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(
				"return Array.from(document.querySelectorAll('[data-test-workspace] [data-test-person]'))" +
				".findIndex(p => p.textContent.includes('" + name + "')) === -1", "True");
		}

		[When("I remove '(.*)' from the workspace")]
		public void IRemoveXFromWorkspace(string name)
		{
			Browser.Interactions.Javascript_IsFlaky(@"
				Array.from(
					document.querySelectorAll('[data-test-workspace] [data-test-person]'))
				.forEach(row => {
				if(row.textContent.includes('" + name + @"'))
					row.querySelector('[data-test-person-remove]').click()
				})");
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-workspace] [data-test-person]'))
				.findIndex(p => p.textContent.includes('" + name + "')) === -1", "True");
		}
	}
}