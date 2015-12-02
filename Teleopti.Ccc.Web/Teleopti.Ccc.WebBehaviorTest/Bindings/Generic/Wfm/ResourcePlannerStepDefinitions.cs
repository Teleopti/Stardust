using System;
using System.Threading;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.DoNotUse;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic.Wfm
{
	[Binding]
	public class ResourcePlannerStepDefinitions
	{
		[Then(@"I should see planning period from '(.*)'to '(.*)'"), SetCulture("sv-SE")]
		public void ThenIShouldSeePlanningPeriodFromTo(DateTime fromDate, DateTime toDate)
		{
			Browser.Interactions.AssertAnyContains(".wfm-card", fromDate.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertAnyContains(".wfm-card", toDate.ToString("yyyy-MM-dd"));
		}

		[Then(@"I should see updated period from '(.*)'to '(.*)'"), SetCulture("sv-SE")]
		public void ThenIShouldSeeUpdatedPeriodFromTo(DateTime fromDate, DateTime toDate)
		{
			Browser.Interactions.AssertAnyContains(".wfm-accordion", fromDate.ToString("yyyy-MM-dd"));
			Browser.Interactions.AssertAnyContains(".wfm-accordion", toDate.ToString("yyyy-MM-dd"));
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
		}

		[When(@"I create new planning period")]
		public void WhenICreateNewPlanningPeriod()
		{
			Browser.Interactions.AssertExists(".wfm-btn:enabled");
			Browser.Interactions.Click(".wfm-btn");
			Browser.Interactions.AssertUrlContains("resourceplanner/planningperiod");
		}


		[When(@"I open planning period")]
		public void WhenIOpenPlanningPeriod()
		{
			Browser.Interactions.Click(".wfm-card");
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
		}

		[Given(@"GroupingReadModel is updated")]
		public void GivenGroupingReadModelIsUpdated()
		{
			DataMaker.Data().ApplyLater(new GroupingReadOnlyUpdate());
		}


	}
}