using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	public sealed class RoleStepDefinition
	{
		[Given("Person '(.*) has role '(.*)'")]
		public void PersonXHasRole(string name, string roleName)
		{
			var role = new RoleForUser
			{
				Name = roleName
			};
			DataMaker.Person(name).Apply(role);
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
				.find(row => row.querySelector('[data-test-person-firstname]').textContent.includes('" + name + @"'))
				.querySelector('[data-test-person-role]').textContent.includes('"+ rolename +"')", "True");
		}

		[Then("Person '(.*)' should not have role '(.*)'")]
		public void PersonShouldNotHaveRoleX(string name, string rolename)
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelectorAll('[data-test-search] [data-test-person]'))
				.find(row => row.querySelector('[data-test-person-firstname]').textContent.includes('" + name + @"'))
				.querySelector('[data-test-person-role]').textContent.includes('" + rolename + "')", "False");
		}
	}
}