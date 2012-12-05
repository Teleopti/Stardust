using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
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

		[When(@"I view Add Shift Trade Request")]
		public void WhenIViewAddShiftTradeRequest()
		{
			TestControllerMethods.Logon();
			Navigation.GotoRequests();
			Pages.Pages.RequestsPage.ShiftTradeRequestsLink.EventualClick();
		}

		[Then(@"I should see a message text saying I am missing a workflow control set")]
		public void ThenIShouldSeeAMessageTextSayingIAmMissingAWorkflowControlSet()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.FriendlyMessage.DisplayVisible(), Is.True);
		}
	}
}
