using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

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

	}
}
