using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestFromRequestStepDefinitions
	{
		[Then(@"Details should be closed")]
		public void ThenDetailsShouldBeClosed()
		{
			Browser.Interactions.AssertNotExists(".request-list", "#Request-detail-section");
		}
	}
}
