using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	public sealed class ApplicationLogonStepDefinition
	{
		[Given("Person '(.*)' has app logon name '(.*)'")]
		public void PersonXHasAppLogonNameY(string name, string logonName)
		{
			DataMaker.Person(name).Set(logonName);
		}


		[When("I navigate to application logon page")]
		public void INavigateToApplicationLogon()
		{
			Browser.Interactions.AssertExists("[data-test-applicationlogon-button]");
			Browser.Interactions.Javascript("document.querySelector('[data-test-applicationlogon-button]').click()");
		}


		[When("I change Application logon for '(.*)' into '(.*)'")]
		public void IChangeApplicationLogonFor(string name, string appLogon)
		{
			Browser.Interactions.Javascript($@"
				Array.from(document.querySelectorAll('[data-test-person]'))
				.find(r => r.textContent.includes('{name}'))
				.value = '{appLogon}'	
			");
		}


		[Then("The application logon page is shown")]
		public void ApplicationLogonPageIsShown()
		{
			Browser.Interactions.AssertExists("[data-test-application-logon]");
		}


		[Then("I can see current application logons")]
		public void ICanSeeCurrentApplicationLogons(Table table)
		{
			foreach (var row in table.Rows)
			{
				row.TryGetValue("Name", out string name);
				row.TryGetValue("AppLogon", out string logonName);

				Browser.Interactions.AssertJavascriptResultContains($@"
					return Array.from(document.querySelectorAll('[data-test-person]'))
					.find(r => r.textContent.includes('{name}'))
					.querySelector('[data-test-person-logon] input').value.includes('{logonName}')
				", "True");
			}
		}


		[Then("I should not the see the application logon page")]
		public void ApplicationLogonPageIsNotShown()
		{
			Browser.Interactions.AssertNotExists("", "[data-test-application-logon]");
		}

	}
}
