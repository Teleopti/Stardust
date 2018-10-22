﻿using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner
{
    [Binding]
    public class ManageScheduleStepDefinition
    {
		[BeforeFeature("RunningStardust")]
		public static void BeforePasswordPolicyScenario()
		{
			TestSiteConfigurationSetup.StartStardust();
		}

		[AfterFeature("RunningStardust")]
		public static void AfterPasswordPolicyScenario()
		{
			TestSiteConfigurationSetup.KillStardust();
		}

		[When(@"I wait (.*) seconds to allow tracking to be setup")]
		public void WhenIWaitSeconds(int seconds)
		{
			Thread.Sleep(TimeSpan.FromSeconds(seconds));
		}

		[When(@"I select '(.*)' as to scenario")]
        public void WhenISelectAsToScenario(string scenario)
        {
			Browser.Interactions.Click(".to-scenario-selector");
			Browser.Interactions.ClickContaining("md-option", scenario);
		}

		[When(@"I select '(.*)' as from scenario")]
		public void WhenISelectAsFromScenario(string scenario)
		{
			Browser.Interactions.Click(".from-scenario-selector");
			Browser.Interactions.ClickContaining("md-option", scenario);
		}

		[When(@"I select the team '(.*)'")]
		public void WhenISelectTheTeam(string team)
		{
			Browser.Interactions.ClickVisibleOnly(".toggle-handle");
			Browser.Interactions.ClickContaining(".tree-handle-wrapper", team);
		}

		[When(@"I run archiving")]
		[When(@"I run importing")]
        public void WhenIRunArchiving()
		{
			Browser.Interactions.Click("#manage-btn");
		}

		[When(@"I confirm to run archiving")]
		[When(@"I confirm to run importing")]
        public void WhenIConfirmToRunArchiving()
		{
			Browser.Interactions.Click("#confirm-managing");
        }

		[Then(@"I should get a success message")]
        public void ThenIShouldGetASuccessMessage()
        {
			using (Browser.TimeoutScope(TimeSpan.FromSeconds(180)))
			{
				Browser.Interactions.AssertExists(".notice-success");
			}
		}
    }
}
