using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
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

		[Given("WA People exists")]
		public void WAPeopleExists(Table table)
		{
			var names = table.AsStrings("Name");
			foreach (var name in names)
				WAPersonXExists(name);
		}

		[Given("Person '(.*) has role '(.*)'")]
		public void PersonXHasRole(string name, string roleName)
		{
			var role = new RoleForUser
			{
				Name = roleName
			};
			DataMaker.Person(name).Apply(role);
		}

		[Given("I have selected person '(.*)'")]
		[When("I select person '(.*)'")]
		public void ISelectPersonX(string name)
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person] [data-test-person-name]'))
				.findIndex(p => p.textContent.includes('" + name + "')) !== -1", "True");
			Browser.Interactions.Javascript(@"
				Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person] [data-test-person-name]'))
				.forEach(p => {if(p.textContent.includes('" + name+"')) p.click()})");
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
			Browser.Interactions.Javascript(@"
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

		[When("I navigate to grant page")]
		public void INavigateToGrant()
		{
			Browser.Interactions.AssertExists("[data-test-grant-button]");
			Browser.Interactions.Javascript("document.querySelector('[data-test-grant-button]').click()");
		}

		[When("I navigate to revoke page")]
		public void INavigateToRevoke()
		{
			Browser.Interactions.AssertExists("[data-test-revoke-button]");
			Browser.Interactions.Javascript("document.querySelector('[data-test-revoke-button]').click()");
		}

		[Then("The grant page is shown")]
		public void GrantPageIsShown()
		{
			Browser.Interactions.AssertExists("[data-test-grant-current]");
		}

		[Then("The revoke page is shown")]
		public void RevokePageIsShown()
		{
			Browser.Interactions.AssertExists("[data-test-revoke-current]");
		}

		[When("I select the role '(.*)' to (grant|revoke)")]
		public void ISelectRoleX(string rolename, string grantOrRevoke)
		{
			var selector = "";
			switch (grantOrRevoke)
			{
				case "grant":
					selector = "[data-test-grant-available] [data-test-chip]";
					break;
				case "revoke":
					selector = "[data-test-revoke-current] [data-test-chip]";
					break;
			}
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('"+selector+@"'))
				.findIndex(p => p.textContent.includes('" + rolename + "')) !== -1", "True");
			Browser.Interactions.Javascript(@"
				Array.from(
					document.querySelectorAll('"+selector+@"'))
				.forEach(p => {if(p.textContent.includes('" + rolename + "')) p.click()})");
		}

		[When("I press the save button")]
		public void WhenISave()
		{
			Browser.Interactions.AssertExists("[data-test-save]");
			Browser.Interactions.Javascript("document.querySelector('[data-test-save]').click()");
		}

		[Then("Person '(.*)' should have role '(.*)'")]
		public void PersonShouldHaveRoleX(string name, string rolename)
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person]'))
				.find(row => row.querySelector('[data-test-person-name]').textContent.includes('" + name + @"'))
				.querySelector('[data-test-person-role]').textContent.includes('"+ rolename +"')", "True");
		}

		[Then("Person '(.*)' should not have role '(.*)'")]
		public void PersonShouldNotHaveRoleX(string name, string rolename)
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person]'))
				.find(row => row.querySelector('[data-test-person-name]').textContent.includes('" + name + @"'))
				.querySelector('[data-test-person-role]').textContent.includes('" + rolename + "')", "False");
		}
	}
}