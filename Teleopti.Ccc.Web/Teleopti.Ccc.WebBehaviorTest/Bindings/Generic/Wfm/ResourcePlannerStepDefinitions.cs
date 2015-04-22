using System;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class ResourcePlannerStepDefinitions
	{
		[Then(@"I should see planning period from '(.*)'to '(.*)'")]
		public void ThenIShouldSeePlanningPeriodFromTo(DateTime fromDate, DateTime toDate)
		{
			Browser.Interactions.AssertAnyContains(".wfm-accordion-head", fromDate.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertAnyContains(".wfm-accordion-head", toDate.ToString("yyyy-MM-dd"));
		}

		[Then(@"I should see '(.*)' are days scheduled")]
		public void ThenIShouldSeeAreDaysScheduled(int days)
		{
			Browser.Interactions.AssertAnyContains(".schedule-days-message",days.ToString());
		}

	}
}