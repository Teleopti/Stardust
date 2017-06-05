using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Wfm.ResourcePlanner
{
	[Binding]
	public class PlanningPeriodStepDefinitions
	{
		[When(@"I click create planning period")]
		public void WhenIClickCreatePlanningPeriod()
		{
			Browser.Interactions.Click(".create-planning-period");
		}

		[When(@"I click apply planning period")]
		public void WhenIClickApplyPlanningPeriod()
		{
			Browser.Interactions.Click(".apply-planning-period");
		}

		[When(@"I click create next planning period")]
		public void WhenIClickCreateNextPlanningPeriod()
		{
			Browser.Interactions.Click(".create-next-planning-period");
		}


		[Then(@"I should see a planning period between '(.*)' and '(.*)'" ), SetCulture("sv-SE")]
		public void ThenIShouldSeeAPlanningPeriodBetweenAnd(string from, string to)
		{
			Browser.Interactions.AssertAnyContains(".plan-group-pp > div.list-header h1", $"{from} - {to}");
		}
		
	}
}