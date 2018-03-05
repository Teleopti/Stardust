using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.TestCommon.Web.WebInteractions.BrowserDriver;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner
{
    [Binding]
    public class ManageScheduleStepDefinition
    {
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

		[When(@"I pick new dates")]
		public void WhenIPickNewDates()
		{
			// not very pretty
			Browser.Interactions.ClickUsingJQuery("date-range-picker .popup-control");
			Browser.Interactions.ClickUsingJQuery(".wfm-datepicker-popup-row > div:not(.ng-hide) .date-range-start-date span:contains(02):first");
			Browser.Interactions.ClickUsingJQuery(".wfm-datepicker-popup-row > div:not(.ng-hide) .date-range-end-date span:contains(02):first");
			Browser.Interactions.ClickUsingJQuery("date-range-picker .popup-control");
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
            Browser.Interactions.AssertExists(".notice-success");
        }
    }
}
