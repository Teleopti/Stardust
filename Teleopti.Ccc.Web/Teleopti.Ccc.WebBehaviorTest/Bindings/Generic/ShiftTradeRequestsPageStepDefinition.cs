using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestsPageStepDefinition
	{
		[Then(@"shift trade requests button should not be visible")]
		public void ThenShiftTradeRequestsButtonShouldNotBeVisible()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddRequestButton.SafeExists(), Is.True);
		}
	}
}
