using System;
using System.Threading;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;
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
			Browser.Interactions.AssertAnyContains(".schedule-days-message",days.ToString());
		}

		[When(@"I click schedule")]
		public void WhenIClickSchedule()
		{
			Browser.Interactions.Click(".schedule-button");
		}

		[When(@"I click next planning period")]
		public void WhenIClickNextPlanningPeriod()
		{
			Browser.Interactions.Click(".next-planning-peroid");
		}

		//[When(@"I update planning period from '(.*)' to '(.*)'")]
		//public void WhenIUpdatePlanningPeriodFromTo(DateTime oldFromDate, DateTime newFromDate)
		//{
		//	var dayValue = (newFromDate - oldFromDate).Days + 1;
		//	Browser.Interactions.ClickContaining(".btn-default", dayValue.ToString());
		//	Thread.Sleep(2000);
		//}

		[When(@"I update planning period to '(.*)' '(.*)'")]
		public void WhenIUpdatePlanningPeriodTo(int p0, string p1)
		{
			Browser.Interactions.Javascript("document.querySelector('.select-num-resourceplanner').value = '" + "1" + "';");
			Browser.Interactions.Javascript("document.querySelector('.select-type-resourceplanner').value = '" + "1" + "';");
		}

		[Given(@"GroupingReadModel is updated")]
		public void GivenGroupingReadModelIsUpdated()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
		}


	}
}