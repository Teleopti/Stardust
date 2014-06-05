using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.BrowserDriver;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestFromRequestStepDefinitions
	{
		[Then(@"Details should be closed")]
		public void ThenDetailsShouldBeClosed()
		{
			Browser.Interactions.AssertNotVisibleUsingJQuery("#Request-detail-section");
		}
	}
}
