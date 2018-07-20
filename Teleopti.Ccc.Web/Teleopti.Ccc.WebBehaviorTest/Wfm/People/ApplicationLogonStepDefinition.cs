using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Infrastructure.Security;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using PersonPeriodConfigurable = Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable.PersonPeriodConfigurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.People
{
	[Binding]
	public sealed class ApplicationLogonStepDefinition
	{
		[Given("Person '(.*)' has app logon name '(.*)'")]
		public void PersonXHasAppLogonNameY(string name, string logonName)
		{
			var person = DataMaker.Person(name);

			using (LocalSystem.TenantUnitOfWork.EnsureUnitOfWorkIsStarted())
			{
				var tenant = LocalSystem.CurrentTenantSession.CurrentSession().Query<Tenant>().FirstOrDefault();
				var pi = new PersonInfo(tenant, person.Person.Id.GetValueOrDefault());
				pi.SetApplicationLogonCredentials(new CheckPasswordStrengthFake(), logonName, "paZZwordz", new OneWayEncryption());
				new PersistTenant(LocalSystem.CurrentTenantSession).Persist(pi);
			}
		}


		[When("I navigate to application logon page")]
		public void INavigateToApplicationLogon()
		{
			Browser.Interactions.AssertExists("[data-test-applicationlogon-button]:not([disabled])");
			Browser.Interactions.Javascript_IsFlaky("document.querySelector('[data-test-applicationlogon-button]').click()");
		}


		[When("I change Application logon for '(.*)' into '(.*)'")]
		public void IChangeApplicationLogonFor(string name, string appLogon)
		{
			Browser.Interactions.Javascript_IsFlaky($@"
				Array.from(document.querySelectorAll('[data-test-person]'))
					.find(r => r.querySelector('[data-test-person-name]').value.includes('{name}'))
					.querySelector('[data-test-person-logon]').value = '{appLogon}'
			");
		}


		[Then("The application logon page is shown")]
		public void ApplicationLogonPageIsShown()
		{
			Browser.Interactions.AssertExists("[data-test-application-logon]");
			Browser.Interactions.AssertUrlContains("access/applicationlogon");
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
					.find(r => r.querySelector('[data-test-person-name]').textContent.includes('{name}'))
					.querySelector('[data-test-person-logon]').value.includes('{logonName}')
				", "True");
			}
		}


		[Then("I should not the see the application logon page")]
		public void ApplicationLogonPageIsNotShown()
		{
			Browser.Interactions.AssertUrlNotContains("","access/applicationlogon");
		}

	}
}
