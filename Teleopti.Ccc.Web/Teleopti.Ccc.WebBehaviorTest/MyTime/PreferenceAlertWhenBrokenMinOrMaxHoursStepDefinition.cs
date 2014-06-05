using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.MyTime
{
	[Binding]
	public class PreferenceAlertWhenBrokenMinOrMaxHoursStepDefinition
	{
		[Then(@"I should see min hours per week as '(.*)'")]
		public void ThenIShouldSeeMinHoursPerWeekAs(String minHours)
		{
			Browser.Interactions.AssertAnyContains(".min-hours-per-week", minHours);
		}

		[Then(@"I should see max hours per week as '(.*)'")]
		public void ThenIShouldSeeMaxHoursPerWeekAs(String maxHours)
		{
			Browser.Interactions.AssertAnyContains(".max-hours-per-week", maxHours);
		}

		[Then(@"I should be alerted for the max hours")]
		[Then(@"I should be alerted for the min hours")]
		public void ThenIShouldBeAlertedForTheMaxHours()
		{
			 Browser.Interactions.AssertExists(".weekly-work-time-alert");
		}

		[Then(@"I should not see min and max hours per week for one week before")]
		public void ThenIShouldNotSeeMinAndMaxHoursPerWeekForOneWeekBefore()
		{
			Browser.Interactions.AssertFirstNotContains("li[data-mytime-week]", ".min-hours-per-week");
		}

		[Then(@"I should not be alerted")]
		public void ThenIShouldNotBeAlerted()
		{
			 Browser.Interactions.AssertNotExists(".max-hours-per-week", ".weekly-work-time-alert");
		}

		[Then(@"I should see warning text in top view")]
		public void ThenIShouldSeeWarningTextInTopView()
		{
			Browser.Interactions.AssertExists("#weekly-work-time-broken");
		}

		[Then(@"I should not see warning text in top view")]
		public void ThenIShouldNotSeeWarningTextInTopView()
		{
			Browser.Interactions.AssertNotExists("#Preference-period-feedback-view", "#weekly-work-time-broken");
		}

	}
}
