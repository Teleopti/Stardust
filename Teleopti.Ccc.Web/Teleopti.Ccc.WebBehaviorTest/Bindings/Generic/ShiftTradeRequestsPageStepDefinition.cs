using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestsPageStepDefinition
	{
		[Then(@"I should not see the Create Shift Trade Request button")]
		public void ThenIShouldNotSeeTheCreateShiftTradeRequestButton()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.AddShiftTradeRequestButton.SafeExists(), Is.False);
		}

		[Then(@"I should not see the Requests button")]
		public void ThenIShouldNotSeeTheRequestsButton()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.ShowRequestsButton.SafeExists(), Is.False);
		}
	}
}
