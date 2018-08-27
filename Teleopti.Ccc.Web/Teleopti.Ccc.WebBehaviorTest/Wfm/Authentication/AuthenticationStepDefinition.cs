using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.Config;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.TestCommon.TestData.Setups.Configurable;
using Teleopti.Ccc.TestCommon.TestData.Setups.Default;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.WebBehaviorTest.Bindings.DoNotUse;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Configurable;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.Authentication
{
	[Binding]
	public sealed class AuthenticationStepDefinition
	{
		private void SignInApplication(string username, string password)
		{
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Username-input", username);
			Browser.Interactions.TypeTextIntoInputTextUsingJQuery("#Password-input", password);
			Browser.Interactions.Click("#Signin-button");
		}

		[Given("I am an agent with default password")]
		public void IAmAnAgentWithDefaultPassword()
		{
			var me = DataMaker.Me();
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
		public void IEnterXInInputY(string text, string selector)
		{
			if (text.ToLower() == "defaultpassword")
			{
				text = DefaultPassword.ThePassword;
			} 
			Browser.Interactions.AssertExists("[" +  selector + "]");
			Browser.Interactions.FillWith("[" +  selector + "]", text);
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
				return document.querySelector('[data-test-change-password-modal]')
					.getAttribute('ng-reflect-nz-visible')
				", "false");
		}
		
	}
}
