using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

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

		[Then(@"I should see '(.*)'")]
		public void ThenIShouldSee(int days)
		{
			Browser.Interactions.AssertAnyContains("p", days +" agents were successfully scheduled");
		}

		[When(@"I click schedule")]
		public void WhenIClickSchedule()
		{
			Browser.Interactions.Click(".schedule-button");
			Thread.Sleep(2000);
		}

		[When(@"I click next planning period")]
		public void WhenIClickNextPlanningPeriod()
		{
			Browser.Interactions.Click(".next-planning-peroid");
		}

		[When(@"I update planning period to two week")]
		public void WhenIUpdatePlanningPeriodToWeek()
		{
			Browser.Interactions.ClickContaining(".wfm-radio-label","2 Week");
			Thread.Sleep(2000);
		}

		[Given(@"GroupingReadModel is updated")]
		public void GivenGroupingReadModelIsUpdated()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
		}


	}
}