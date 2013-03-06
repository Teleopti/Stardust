using System.Linq;
using NUnit.Framework;
using TechTalk.SpecFlow;
using Teleopti.Ccc.WebBehaviorTest.Core;
using Teleopti.Ccc.WebBehaviorTest.Core.Extensions;
using Teleopti.Ccc.WebBehaviorTest.Core.Robustness;
using Teleopti.Ccc.WebBehaviorTest.Data;
using Teleopti.Ccc.WebBehaviorTest.Data.Setups.Specific;

namespace Teleopti.Ccc.WebBehaviorTest.Bindings.Generic
{
	[Binding]
	public class ShiftTradeRequestFromRequestStepDefinitions
	{
		[Given(@"I click the shift trade request's delete button")]
		[When(@"I click the shift trade request's delete button")]
		public void GivenIClickTheShiftTradeRequestSDeleteButton()
		{
			var requestId = UserFactory.User().UserData<ExistingShiftTradeRequest>().PersonRequest.Id.Value;
			Pages.Pages.RequestsPage.RequestDeleteButtonById(requestId).EventualClick();
		}

		[Then(@"I should not see the shift trade request in the list")]
		public void ThenIShouldNotSeeTheShiftTradeRequestInTheList()
		{
			var existingShiftTradeRequest = UserFactory.User().UserData<ExistingShiftTradeRequest>();
			if (existingShiftTradeRequest != null)
			{
				var requestId = existingShiftTradeRequest.PersonRequest.Id.Value;
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				Navigation.GotoRequests();
				EventualAssert.That(() => Pages.Pages.RequestsPage.RequestById(requestId).Exists, Is.False);
				return;
			}
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(), Is.EqualTo(0));
		}

		[Then(@"Details should be closed")]
		public void ThenDetailsShouldBeClosed()
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDetailSection.IsDisplayed(), Is.False, "The detailsection should not be visible");
		}

		[Then(@"Shift trade request with subject '(.*)' should not be displayed in the list")]
		public void ThenShiftTradeRequestWithSubjectShouldNotBeDisplayed(string subject)
		{
			EventualAssert.That(() => Pages.Pages.RequestsPage.Requests.Count(r=>r.InnerHtml.Contains(subject)), Is.Empty,"It should not be shown in the list");
			EventualAssert.That(() => Pages.Pages.RequestsPage.RequestDetailSection.IsDisplayed(), Is.True, "The detailsection should be visible");
		}

	}
}
