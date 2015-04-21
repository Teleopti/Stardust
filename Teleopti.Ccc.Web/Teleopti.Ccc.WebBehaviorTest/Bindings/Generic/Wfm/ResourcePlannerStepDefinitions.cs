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
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".ng-binding:contains('{0}')", fromDate.ToString("yyyy-MM-dd")));
			Browser.Interactions.AssertExistsUsingJQuery(string.Format(".ng-binding:contains('{0}')", toDate.ToString("yyyy-MM-dd")));
		}
	}
}