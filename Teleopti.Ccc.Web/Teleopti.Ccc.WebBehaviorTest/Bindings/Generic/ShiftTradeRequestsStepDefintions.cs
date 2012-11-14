using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Pages;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestsStepDefintions
	{
		private ShiftTradeRequestsPage _page { get { return Pages.Pages.ShiftTradeRequestsPage; } }

		[Then(@"shift trade tab should not be visible")]
		public void ThenShiftTradeTabShouldNotBeVisible()
		{
			EventualAssert.That(() => _page.ShiftTradeRequestsLink.Exists, Is.False);
		}
	}
}
