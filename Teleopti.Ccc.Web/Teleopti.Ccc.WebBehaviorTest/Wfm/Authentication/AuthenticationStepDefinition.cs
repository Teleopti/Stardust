using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Authentication
{
	[Binding]
	public sealed class AuthenticationStepDefinition
	{

		[Given("I am an agent with password '(.*)'")]
		public void IAmAnAgentWithPassword(string password)
		{
			DataMaker.Data().RemoveLastPerson();
			DataMaker.Data().Person("agent")
				.Apply(new PersonUserConfigurable
				{
					UserName = "temp",
					Password = password
				});
		}

		[Given("I navigate to change your password")]
		public void INavigateToChangeYourPassword()
		{
			Browser.Interactions.AssertExists("[data-test-nav-item-settings]");
			Browser.Interactions.PressEnter("[data-test-nav-item-settings]");
			Browser.Interactions.AssertExists("[data-test-change-password-button]");
			Browser.Interactions.PressEnter("[data-test-change-password-button]");
		}

		[Given("I enter '(.*)' in '(.*)'")]
		public void IEnterXInInputY(string password, string selector)
		{
			Browser.Interactions.AssertExists("[" +  selector + "]");
			Browser.Interactions.FillWith("[" +  selector + "]", password);
		}

		[When("I click Ok")]
		public void IClickOk()
		{
			Browser.Interactions.AssertExists("[data-test-change-password-modal] .ant-btn-primary");
			Browser.Interactions.PressEnter("[data-test-change-password-modal] .ant-btn-primary");
		}

		[Then("The password modal should close")]
		public void ThePasswordModalShouldClose()
		{
			Browser.Interactions.AssertJavascriptResultContains(@"
				return Array.from(
					document.querySelector('[data-test-change-password-modal]')
					.getAttribute('ng-reflect-nz-visible')
				", "false");
		}
		
	}
}
